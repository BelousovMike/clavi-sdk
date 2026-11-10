package clavi

import (
	"fmt"
	"net/url"
	"os"
	"strings"
)

// Config is how the client finds Clavi.
// BaseURL is required (or set CLAVI_BASE_URL). TenantHost defaults to the URL host.
type Config struct {
	BaseURL                    string
	TenantHost                 string
	Origin                     string
	AcceptAnyServerCertificate bool
}

func (c Config) withDefaults() (Config, error) {
	base := c.BaseURL
	if base == "" {
		base = os.Getenv("CLAVI_BASE_URL")
	}
	base = strings.TrimRight(base, "/") + "/"
	if base == "/" {
		return c, fmt.Errorf("base_url / CLAVI_BASE_URL is required")
	}
	u, err := url.Parse(base)
	if err != nil {
		return c, err
	}
	host := c.TenantHost
	if host == "" {
		host = os.Getenv("CLAVI_TENANT_HOST")
	}
	if host == "" {
		host = u.Host
	}
	origin := c.Origin
	if origin == "" {
		origin = os.Getenv("CLAVI_ORIGIN")
	}
	if origin == "" {
		origin = u.Scheme + "://" + u.Host
	}
	c.BaseURL = base
	c.TenantHost = host
	c.Origin = origin
	return c, nil
}
