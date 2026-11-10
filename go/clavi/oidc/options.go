package oidc

import (
	"fmt"
	"os"
	"strings"
)

// Options mirrors Clavi.Sdk.AspNetCore ClaviOidcOptions / docs/oidc-adapters.md.
type Options struct {
	Authority                   string
	ClientID                    string
	ClientSecret                string
	CallbackPath                string
	SignedOutCallbackPath       string
	Scopes                      []string
	MapRoles                    bool
	AcceptAnyServerCertificate  bool
	DisablePushedAuthorization  bool
	BaseURL                     string
	TenantHost                  string
}

func defaults() Options {
	return Options{
		CallbackPath:               "/signin-oidc",
		SignedOutCallbackPath:      "/signout-callback-oidc",
		Scopes:                     []string{"openid", "profile", "email"},
		MapRoles:                   true,
		DisablePushedAuthorization: true,
	}
}

// FromEnv loads CLAVI_* / Clavi__* into Options.
func FromEnv() Options {
	o := defaults()
	o.Authority = firstEnv("CLAVI_AUTHORITY", "Clavi__Authority")
	o.ClientID = firstEnv("CLAVI_CLIENT_ID", "Clavi__ClientId")
	o.ClientSecret = firstEnv("CLAVI_CLIENT_SECRET", "Clavi__ClientSecret")
	if v := firstEnv("CLAVI_CALLBACK_PATH", "Clavi__CallbackPath"); v != "" {
		o.CallbackPath = v
	}
	if v := firstEnv("CLAVI_SIGNED_OUT_CALLBACK_PATH", "Clavi__SignedOutCallbackPath"); v != "" {
		o.SignedOutCallbackPath = v
	}
	if v := firstEnv("CLAVI_SCOPES", "Clavi__Scopes"); v != "" {
		o.Scopes = strings.Fields(strings.ReplaceAll(v, ",", " "))
	}
	o.BaseURL = firstEnv("CLAVI_BASE_URL", "Clavi__BaseUrl")
	o.TenantHost = firstEnv("CLAVI_TENANT_HOST", "Clavi__TenantHost")
	return o
}

// Merge overlays non-zero fields from partial onto base.
func Merge(base Options, partial Options) Options {
	out := base
	if partial.Authority != "" {
		out.Authority = strings.TrimRight(partial.Authority, "/")
	}
	if partial.ClientID != "" {
		out.ClientID = partial.ClientID
	}
	if partial.ClientSecret != "" {
		out.ClientSecret = partial.ClientSecret
	}
	if partial.CallbackPath != "" {
		out.CallbackPath = partial.CallbackPath
	}
	if partial.SignedOutCallbackPath != "" {
		out.SignedOutCallbackPath = partial.SignedOutCallbackPath
	}
	if len(partial.Scopes) > 0 {
		out.Scopes = partial.Scopes
	}
	if partial.BaseURL != "" {
		out.BaseURL = partial.BaseURL
	}
	if partial.TenantHost != "" {
		out.TenantHost = partial.TenantHost
	}
	// Bools: keep base defaults; AcceptAny can only be turned on by partial.
	if partial.AcceptAnyServerCertificate {
		out.AcceptAnyServerCertificate = true
	}
	return out
}

func (o Options) Validate() error {
	if strings.TrimSpace(o.Authority) == "" {
		return fmt.Errorf("clavi: Authority is required")
	}
	if strings.TrimSpace(o.ClientID) == "" {
		return fmt.Errorf("clavi: ClientID is required")
	}
	return nil
}

func (o Options) AdminBaseURL() (string, error) {
	raw := o.BaseURL
	if raw == "" {
		raw = o.Authority
	}
	raw = strings.TrimRight(raw, "/") + "/"
	if raw == "/" {
		return "", fmt.Errorf("clavi: BaseURL or Authority required for Admin")
	}
	return raw, nil
}

func firstEnv(names ...string) string {
	for _, n := range names {
		if v := os.Getenv(n); v != "" {
			return v
		}
	}
	return ""
}
