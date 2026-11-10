package oidc

import (
	"encoding/base64"
	"testing"
)

func TestValidateAndAdminBaseURL(t *testing.T) {
	o := Merge(defaults(), Options{Authority: "https://clavi.example/", ClientID: "app"})
	if err := o.Validate(); err != nil {
		t.Fatal(err)
	}
	base, err := o.AdminBaseURL()
	if err != nil {
		t.Fatal(err)
	}
	if base != "https://clavi.example/" {
		t.Fatalf("got %q", base)
	}
}

func TestRolesFrom(t *testing.T) {
	payload := base64.RawURLEncoding.EncodeToString([]byte(`{"roles":["a","b"]}`))
	token := "x." + payload + ".y"
	roles := rolesFrom(map[string]any{"roles": []any{"b", "c"}}, token, true)
	if len(roles) != 3 {
		t.Fatalf("roles=%v", roles)
	}
}
