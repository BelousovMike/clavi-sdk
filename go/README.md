# clavi (Go)

Admin API client for Clavi. Create a client, `Auth`, then call `Users`, `Groups`, and friends.

App OIDC middleware: package `github.com/BelousovMike/clavi-sdk/go/clavi/oidc` — see `clavi/oidc/README.md` and `../docs/oidc-adapters.md`.

```go
import "github.com/BelousovMike/clavi-sdk/go/clavi"

c, err := clavi.New(clavi.Config{BaseURL: "https://clavi.example/"})
if err != nil {
    return err
}
if err := c.Auth("admin", password); err != nil {
    return err
}
page, err := c.Users.Find(1, 20, "")
```

For local compose with a self-signed cert, set `AcceptAnyServerCertificate: true`.
