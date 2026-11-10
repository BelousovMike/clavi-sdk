package oidc

import (
	"context"
	"crypto/rand"
	"crypto/sha256"
	"crypto/tls"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"strings"
	"sync"

	"github.com/BelousovMike/clavi-sdk/go/clavi"
)

// Principal is the signed-in Clavi user stored in SessionStore.
type Principal struct {
	Sub         string   `json:"sub"`
	Name        string   `json:"name"`
	Email       string   `json:"email"`
	Roles       []string `json:"roles"`
	IDToken     string   `json:"-"`
	AccessToken string   `json:"-"`
}

// SessionStore persists OIDC pending state and the principal (cookie session, etc.).
type SessionStore interface {
	Get(r *http.Request, key string) (any, bool)
	Set(w http.ResponseWriter, r *http.Request, key string, value any) error
	Delete(w http.ResponseWriter, r *http.Request, key string) error
}

// MemorySession is a demo store keyed by a cookie; not for production clusters.
type MemorySession struct {
	mu    sync.Mutex
	data  map[string]map[string]any
	name  string
}

func NewMemorySession(cookieName string) *MemorySession {
	if cookieName == "" {
		cookieName = "clavi_sid"
	}
	return &MemorySession{data: map[string]map[string]any{}, name: cookieName}
}

func (m *MemorySession) sid(r *http.Request) string {
	c, err := r.Cookie(m.name)
	if err != nil || c.Value == "" {
		return ""
	}
	return c.Value
}

func (m *MemorySession) ensure(w http.ResponseWriter, r *http.Request) string {
	id := m.sid(r)
	if id != "" {
		return id
	}
	b := make([]byte, 16)
	_, _ = rand.Read(b)
	id = base64.RawURLEncoding.EncodeToString(b)
	http.SetCookie(w, &http.Cookie{Name: m.name, Value: id, Path: "/", HttpOnly: true, SameSite: http.SameSiteLaxMode})
	m.mu.Lock()
	m.data[id] = map[string]any{}
	m.mu.Unlock()
	return id
}

func (m *MemorySession) Get(r *http.Request, key string) (any, bool) {
	m.mu.Lock()
	defer m.mu.Unlock()
	s := m.data[m.sid(r)]
	if s == nil {
		return nil, false
	}
	v, ok := s[key]
	return v, ok
}

func (m *MemorySession) Set(w http.ResponseWriter, r *http.Request, key string, value any) error {
	id := m.ensure(w, r)
	m.mu.Lock()
	defer m.mu.Unlock()
	if m.data[id] == nil {
		m.data[id] = map[string]any{}
	}
	m.data[id][key] = value
	return nil
}

func (m *MemorySession) Delete(w http.ResponseWriter, r *http.Request, key string) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	if s := m.data[m.sid(r)]; s != nil {
		delete(s, key)
	}
	return nil
}

// Handler wires login/callback/logout/me for net/http (chi-compatible).
type Handler struct {
	Opts     Options
	Sessions SessionStore
	Admin    *clavi.Client
	Origin   string // optional public origin override

	metaOnce sync.Once
	meta     map[string]any
	metaErr  error
	client   *http.Client
}

// New builds Handler + Admin client from options (env merged when zero).
func New(opts Options) (*Handler, error) {
	merged := Merge(FromEnv(), opts)
	if merged.Authority != "" {
		merged.Authority = strings.TrimRight(merged.Authority, "/")
	}
	if err := merged.Validate(); err != nil {
		return nil, err
	}
	base, err := merged.AdminBaseURL()
	if err != nil {
		return nil, err
	}
	admin, err := clavi.New(clavi.Config{
		BaseURL:                    base,
		TenantHost:                 merged.TenantHost,
		AcceptAnyServerCertificate: merged.AcceptAnyServerCertificate,
	})
	if err != nil {
		return nil, err
	}
	tr := http.DefaultTransport.(*http.Transport).Clone()
	if merged.AcceptAnyServerCertificate {
		tr.TLSClientConfig = &tls.Config{InsecureSkipVerify: true} //nolint:gosec // lab only
	}
	return &Handler{
		Opts:     merged,
		Sessions: NewMemorySession(""),
		Admin:    admin,
		client:   &http.Client{Transport: tr},
	}, nil
}

// Mount registers routes on mux (ServeMux or chi).
func (h *Handler) Mount(mux interface {
	HandleFunc(pattern string, handler func(http.ResponseWriter, *http.Request))
}) {
	mux.HandleFunc("/login", h.Login)
	mux.HandleFunc(h.Opts.CallbackPath, h.Callback)
	mux.HandleFunc("/logout", h.Logout)
	mux.HandleFunc(h.Opts.SignedOutCallbackPath, h.SignedOut)
	mux.HandleFunc("/me", h.Me)
}

// RequireAuth middleware; optional roles.
func (h *Handler) RequireAuth(roles ...string) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			p, ok := h.Principal(r)
			if !ok {
				http.Error(w, `{"error":"unauthorized"}`, http.StatusUnauthorized)
				return
			}
			if len(roles) > 0 {
				allowed := false
				for _, need := range roles {
					for _, have := range p.Roles {
						if have == need {
							allowed = true
							break
						}
					}
				}
				if !allowed {
					http.Error(w, `{"error":"forbidden"}`, http.StatusForbidden)
					return
				}
			}
			ctx := context.WithValue(r.Context(), principalKey{}, p)
			next.ServeHTTP(w, r.WithContext(ctx))
		})
	}
}

type principalKey struct{}

// PrincipalFromContext returns the user set by RequireAuth.
func PrincipalFromContext(ctx context.Context) (Principal, bool) {
	p, ok := ctx.Value(principalKey{}).(Principal)
	return p, ok
}

func (h *Handler) Principal(r *http.Request) (Principal, bool) {
	v, ok := h.Sessions.Get(r, "clavi_user")
	if !ok {
		return Principal{}, false
	}
	p, ok := v.(Principal)
	return p, ok
}

func (h *Handler) Login(w http.ResponseWriter, r *http.Request) {
	meta, err := h.discovery()
	if err != nil {
		http.Error(w, err.Error(), 500)
		return
	}
	verifier, challenge := pkce()
	state := randomToken()
	nonce := randomToken()
	_ = h.Sessions.Set(w, r, "clavi_oidc", map[string]string{
		"verifier":  verifier,
		"state":     state,
		"nonce":     nonce,
		"return_to": r.URL.Query().Get("return_to"),
	})
	q := url.Values{}
	q.Set("client_id", h.Opts.ClientID)
	q.Set("response_type", "code")
	q.Set("redirect_uri", h.abs(r, h.Opts.CallbackPath))
	q.Set("scope", strings.Join(h.Opts.Scopes, " "))
	q.Set("state", state)
	q.Set("nonce", nonce)
	q.Set("code_challenge", challenge)
	q.Set("code_challenge_method", "S256")
	authURL := meta["authorization_endpoint"].(string) + "?" + q.Encode()
	http.Redirect(w, r, authURL, http.StatusFound)
}

func (h *Handler) Callback(w http.ResponseWriter, r *http.Request) {
	raw, ok := h.Sessions.Get(r, "clavi_oidc")
	if !ok {
		http.Error(w, "missing OIDC session", 400)
		return
	}
	pending := raw.(map[string]string)
	if errParam := r.URL.Query().Get("error"); errParam != "" {
		http.Error(w, r.URL.Query().Get("error_description"), 400)
		return
	}
	if r.URL.Query().Get("state") != pending["state"] {
		http.Error(w, "invalid state", 400)
		return
	}
	meta, err := h.discovery()
	if err != nil {
		http.Error(w, err.Error(), 500)
		return
	}
	form := url.Values{}
	form.Set("grant_type", "authorization_code")
	form.Set("client_id", h.Opts.ClientID)
	form.Set("code", r.URL.Query().Get("code"))
	form.Set("redirect_uri", h.abs(r, h.Opts.CallbackPath))
	form.Set("code_verifier", pending["verifier"])
	if h.Opts.ClientSecret != "" {
		form.Set("client_secret", h.Opts.ClientSecret)
	}
	res, err := h.client.PostForm(meta["token_endpoint"].(string), form)
	if err != nil {
		http.Error(w, err.Error(), 500)
		return
	}
	defer res.Body.Close()
	body, _ := io.ReadAll(res.Body)
	if res.StatusCode >= 300 {
		http.Error(w, string(body), res.StatusCode)
		return
	}
	var tokens map[string]any
	_ = json.Unmarshal(body, &tokens)
	access, _ := tokens["access_token"].(string)
	idToken, _ := tokens["id_token"].(string)
	userInfo := map[string]any{}
	if ep, ok := meta["userinfo_endpoint"].(string); ok && ep != "" {
		req, _ := http.NewRequest(http.MethodGet, ep, nil)
		req.Header.Set("Authorization", "Bearer "+access)
		ures, err := h.client.Do(req)
		if err == nil {
			defer ures.Body.Close()
			_ = json.NewDecoder(ures.Body).Decode(&userInfo)
		}
	}
	p := Principal{
		Sub:         str(userInfo["sub"]),
		Name:        firstNonEmpty(str(userInfo["preferred_username"]), str(userInfo["name"]), str(userInfo["email"])),
		Email:       str(userInfo["email"]),
		Roles:       rolesFrom(userInfo, access, h.Opts.MapRoles),
		IDToken:     idToken,
		AccessToken: access,
	}
	_ = h.Sessions.Set(w, r, "clavi_user", p)
	_ = h.Sessions.Delete(w, r, "clavi_oidc")
	ret := pending["return_to"]
	if ret == "" {
		ret = "/"
	}
	http.Redirect(w, r, ret, http.StatusFound)
}

func (h *Handler) Logout(w http.ResponseWriter, r *http.Request) {
	p, _ := h.Principal(r)
	meta, _ := h.discovery()
	_ = h.Sessions.Delete(w, r, "clavi_user")
	_ = h.Sessions.Delete(w, r, "clavi_oidc")
	if ep, ok := meta["end_session_endpoint"].(string); ok && ep != "" {
		q := url.Values{}
		if p.IDToken != "" {
			q.Set("id_token_hint", p.IDToken)
		}
		q.Set("post_logout_redirect_uri", h.abs(r, h.Opts.SignedOutCallbackPath))
		http.Redirect(w, r, ep+"?"+q.Encode(), http.StatusFound)
		return
	}
	http.Redirect(w, r, h.Opts.SignedOutCallbackPath, http.StatusFound)
}

func (h *Handler) SignedOut(w http.ResponseWriter, r *http.Request) {
	http.Redirect(w, r, "/", http.StatusFound)
}

func (h *Handler) Me(w http.ResponseWriter, r *http.Request) {
	p, ok := h.Principal(r)
	if !ok {
		http.Error(w, `{"authenticated":false}`, http.StatusUnauthorized)
		return
	}
	_ = json.NewEncoder(w).Encode(map[string]any{
		"authenticated": true,
		"user": map[string]any{
			"sub": p.Sub, "name": p.Name, "email": p.Email, "roles": p.Roles,
		},
	})
}

func (h *Handler) discovery() (map[string]any, error) {
	h.metaOnce.Do(func() {
		url := h.Opts.Authority + "/.well-known/openid-configuration"
		res, err := h.client.Get(url)
		if err != nil {
			h.metaErr = err
			return
		}
		defer res.Body.Close()
		if res.StatusCode >= 300 {
			h.metaErr = fmt.Errorf("discovery HTTP %d", res.StatusCode)
			return
		}
		h.metaErr = json.NewDecoder(res.Body).Decode(&h.meta)
	})
	return h.meta, h.metaErr
}

func (h *Handler) abs(r *http.Request, path string) string {
	origin := h.Origin
	if origin == "" {
		scheme := "http"
		if r.TLS != nil {
			scheme = "https"
		}
		if xf := r.Header.Get("X-Forwarded-Proto"); xf != "" {
			scheme = xf
		}
		host := r.Host
		if xh := r.Header.Get("X-Forwarded-Host"); xh != "" {
			host = xh
		}
		origin = scheme + "://" + host
	}
	return strings.TrimRight(origin, "/") + path
}

func pkce() (verifier, challenge string) {
	b := make([]byte, 32)
	_, _ = rand.Read(b)
	verifier = base64.RawURLEncoding.EncodeToString(b)
	sum := sha256.Sum256([]byte(verifier))
	challenge = base64.RawURLEncoding.EncodeToString(sum[:])
	return
}

func randomToken() string {
	b := make([]byte, 16)
	_, _ = rand.Read(b)
	return base64.RawURLEncoding.EncodeToString(b)
}

func rolesFrom(userInfo map[string]any, access string, mapRoles bool) []string {
	if !mapRoles {
		return nil
	}
	set := map[string]struct{}{}
	add := func(v any) {
		arr, ok := v.([]any)
		if !ok {
			return
		}
		for _, item := range arr {
			if s, ok := item.(string); ok && strings.TrimSpace(s) != "" {
				set[strings.TrimSpace(s)] = struct{}{}
			}
		}
	}
	add(userInfo["roles"])
	if parts := strings.Split(access, "."); len(parts) >= 2 {
		raw, err := base64.RawURLEncoding.DecodeString(parts[1])
		if err == nil {
			var payload map[string]any
			if json.Unmarshal(raw, &payload) == nil {
				add(payload["roles"])
			}
		}
	}
	out := make([]string, 0, len(set))
	for k := range set {
		out = append(out, k)
	}
	return out
}

func str(v any) string {
	s, _ := v.(string)
	return s
}

func firstNonEmpty(vals ...string) string {
	for _, v := range vals {
		if v != "" {
			return v
		}
	}
	return ""
}
