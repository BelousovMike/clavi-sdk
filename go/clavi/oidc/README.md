# clavi/oidc

`net/http` (chi-compatible) OIDC middleware for Clavi.

```go
h, err := oidc.New(oidc.Options{
  Authority: "https://clavi.example",
  ClientID:  "my-app",
})
mux := http.NewServeMux()
h.Mount(mux)
mux.Handle("/secure", h.RequireAuth("acme.catalog.write")(http.HandlerFunc(...)))
// Admin: h.Admin
```

Config via `CLAVI_AUTHORITY`, `CLAVI_CLIENT_ID`, … — see `docs/oidc-adapters.md`.
