# clavi-sdk

Typed Admin API clients and framework OIDC adapters for Clavi Community (Apache-2.0).

Shared OIDC + Admin config contract: [`docs/oidc-adapters.md`](docs/oidc-adapters.md).

## Packages

| Lang | Package | Role |
|------|---------|------|
| .NET | `Clavi.Sdk.Admin` | Admin API + DI |
| .NET | `Clavi.Sdk.AspNetCore` | OIDC RP (cookie + PKCE + roles) |
| JS | `@clavi/admin-client` | Admin API |
| JS | `@clavi/oidc` | Shared OIDC options / PKCE / roles |
| JS | `@clavi/express` | Express OIDC + Admin |
| JS | `@clavi/nestjs` | NestJS module |
| JS | `@clavi/next` | Auth.js / NextAuth provider |
| Java | `io.clavi:clavi-admin-client` | Admin API |
| Java | `io.clavi:clavi-spring-boot-starter` | Spring Boot OAuth2 + Admin bean |
| Python | `clavi-admin` | Admin API |
| Python | `clavi-oidc` | Shared OIDC helpers |
| Python | `clavi-fastapi` | FastAPI router + Depends |
| Python | `clavi-django` | Django views + decorators |
| Go | `…/go/clavi` | Admin API |
| Go | `…/go/clavi/oidc` | `net/http` middleware (chi-compatible) |

### .NET Admin

```csharp
await using var client = new ClaviAdminClient(new ClaviClientOptions {
    BaseUrl = new Uri("https://clavi.example/")
});
await client.AuthAsync("admin", password);
var page = await client.Users.FindAsync(page: 1, pageSize: 20);
```

Or DI: `services.AddClaviAdmin(configuration.GetSection("Clavi"))`.

### .NET ASP.NET Core (OIDC)

```csharp
builder.Services.AddClaviOpenIdConnect(); // section "Clavi"
// or fluent:
builder.Services.AddClaviOpenIdConnect(c => c
    .ReadFrom.Configuration(builder.Configuration)
    .AcceptAnyServerCertificate(builder.Environment.IsDevelopment()));
```

### JavaScript Admin

```js
import ClaviAdminClient from '@clavi/admin-client';
const client = new ClaviAdminClient({ baseUrl: 'https://clavi.example/' });
await client.auth({ username: 'admin', password });
```

### Express

```js
import { addClaviOpenIdConnect, requireClaviAuth } from '@clavi/express';
addClaviOpenIdConnect(app, (c) => c.readFrom.env().clientId('my-app'));
app.get('/secure', requireClaviAuth(['acme.catalog.write']), handler);
```

### NestJS

```ts
ClaviModule.forRoot((c) => c.readFrom.env());
ClaviNest.attach(expressApp); // OIDC routes
```

### Next.js (Auth.js)

```ts
providers: [ClaviProvider((c) => c.readFrom.env().clientId('my-app'))]
```

### Java

```java
try (var client = ClaviAdminClient.builder()
        .baseUrl("https://clavi.example/")
        .build()) {
    client.auth("admin", password);
}
```

Spring Boot (`application.yml`):

```yaml
clavi:
  authority: https://clavi.example
  client-id: my-app
  base-url: https://clavi.example/
```

### Python

```python
from clavi_admin import ClaviAdminClient
client = ClaviAdminClient(base_url="https://clavi.example/")
client.auth("admin", password)
```

FastAPI:

```python
from clavi_fastapi import add_clavi_openid_connect, require_clavi_auth
add_clavi_openid_connect(app, lambda c: c.read_from_env().client_id("my-app"))
```

Django: add `clavi_django` to `INSTALLED_APPS`, set `CLAVI = {...}`, include `clavi_django.urls`.

### Go

```go
c, err := clavi.New(clavi.Config{BaseURL: "https://clavi.example/"})
h, err := oidc.New(oidc.Options{Authority: "https://clavi.example", ClientID: "my-app"})
h.Mount(mux)
```

OpenAPI pin: `openapi/openapi.v1.json` (refresh with `./scripts/refresh-openapi.sh`).
