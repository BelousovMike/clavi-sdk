package clavi_test

import (
	"os"
	"testing"

	"github.com/BelousovMike/clavi-sdk/go/clavi"
)

func TestNewRequiresBaseURL(t *testing.T) {
	t.Setenv("CLAVI_BASE_URL", "")
	_, err := clavi.New(clavi.Config{})
	if err == nil {
		t.Fatal("expected error")
	}
}

func TestLoginAndListUsers(t *testing.T) {
	if os.Getenv("CLAVI_SDK_SMOKE") != "1" {
		t.Skip("set CLAVI_SDK_SMOKE=1")
	}
	password := os.Getenv("CLAVI_E2E_ADMIN_PASSWORD")
	if password == "" {
		t.Fatal("CLAVI_E2E_ADMIN_PASSWORD required")
	}
	username := envOr("CLAVI_E2E_ADMIN_USERNAME", "admin")
	baseURL := envOr("CLAVI_BASE_URL", "https://clavi.clavi.local/")
	host := envOr("CLAVI_TENANT_HOST", "clavi.clavi.local")

	c, err := clavi.New(clavi.Config{
		BaseURL:                    baseURL,
		TenantHost:                 host,
		AcceptAnyServerCertificate: true,
	})
	if err != nil {
		t.Fatal(err)
	}
	if err := c.Auth(username, password); err != nil {
		t.Fatal(err)
	}
	page, err := c.Users.Find(1, 10, "")
	if err != nil {
		t.Fatal(err)
	}
	if page == nil || page.Items == nil {
		t.Fatal("expected paged users")
	}
	if page.PageSize > 100 {
		t.Fatalf("unexpected pageSize %d", page.PageSize)
	}
}

func envOr(k, def string) string {
	if v := os.Getenv(k); v != "" {
		return v
	}
	return def
}
