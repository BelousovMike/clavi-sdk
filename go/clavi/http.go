package clavi

import (
	"bytes"
	"crypto/tls"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"strings"
	"sync"
)

type httpClient struct {
	cfg     Config
	http    *http.Client
	cookies map[string]string
	mu      sync.Mutex
}

func newHTTP(cfg Config) *httpClient {
	transport := http.DefaultTransport.(*http.Transport).Clone()
	if cfg.AcceptAnyServerCertificate {
		transport.TLSClientConfig = &tls.Config{InsecureSkipVerify: true} //nolint:gosec // local compose
	}
	return &httpClient{
		cfg:     cfg,
		http:    &http.Client{Transport: transport},
		cookies: map[string]string{},
	}
}

func (c *httpClient) get(path string, out any) error {
	return c.do(http.MethodGet, path, nil, out)
}

func (c *httpClient) post(path string, body any, out any) error {
	return c.do(http.MethodPost, path, body, out)
}

func (c *httpClient) delete(path string) error {
	return c.do(http.MethodDelete, path, nil, nil)
}

func (c *httpClient) do(method, rel string, body any, out any) error {
	u, err := url.Parse(c.cfg.BaseURL)
	if err != nil {
		return err
	}
	rel = strings.TrimPrefix(rel, "/")
	full, err := u.Parse(rel)
	if err != nil {
		return err
	}

	var reader io.Reader
	if body != nil {
		b, err := json.Marshal(body)
		if err != nil {
			return err
		}
		reader = bytes.NewReader(b)
	}

	req, err := http.NewRequest(method, full.String(), reader)
	if err != nil {
		return err
	}
	req.Header.Set("Accept", "application/json")
	req.Header.Set("Origin", c.cfg.Origin)
	if !strings.EqualFold(c.cfg.TenantHost, full.Hostname()) {
		req.Host = c.cfg.TenantHost
	}
	if body != nil {
		req.Header.Set("Content-Type", "application/json")
	}
	c.mu.Lock()
	if len(c.cookies) > 0 {
		parts := make([]string, 0, len(c.cookies))
		for k, v := range c.cookies {
			parts = append(parts, k+"="+v)
		}
		req.Header.Set("Cookie", strings.Join(parts, "; "))
	}
	c.mu.Unlock()

	resp, err := c.http.Do(req)
	if err != nil {
		return err
	}
	defer resp.Body.Close()
	c.storeCookies(resp.Header.Values("Set-Cookie"))

	raw, err := io.ReadAll(resp.Body)
	if err != nil {
		return err
	}
	if resp.StatusCode >= 400 {
		code := ""
		var payload map[string]any
		if json.Unmarshal(raw, &payload) == nil {
			if v, ok := payload["code"].(string); ok {
				code = v
			}
		}
		return &APIError{Status: resp.StatusCode, Code: code, Body: string(raw)}
	}
	if out == nil || resp.StatusCode == 204 || len(raw) == 0 {
		return nil
	}
	return json.Unmarshal(raw, out)
}

func (c *httpClient) storeCookies(lines []string) {
	c.mu.Lock()
	defer c.mu.Unlock()
	for _, line := range lines {
		pair := strings.SplitN(line, ";", 2)[0]
		if i := strings.IndexByte(pair, '='); i > 0 {
			c.cookies[strings.TrimSpace(pair[:i])] = strings.TrimSpace(pair[i+1:])
		}
	}
}

func pagedPath(path string, page, pageSize int, search string) string {
	q := url.Values{}
	q.Set("page", fmt.Sprintf("%d", page))
	q.Set("page_size", fmt.Sprintf("%d", pageSize))
	if strings.TrimSpace(search) != "" {
		q.Set("search", strings.TrimSpace(search))
	}
	sep := "?"
	if strings.Contains(path, "?") {
		sep = "&"
	}
	return path + sep + q.Encode()
}
