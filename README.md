# clavi-sdk

Typed Admin API clients for Clavi Community (Apache-2.0).

Same idea in every language: create a client, call `auth` / `Auth`, then use resources like `users` / `Users`. Session cookies stay on the client. Point `baseUrl` at your tenant host (or set `tenantHost` when they differ).

| Lang | Package |
|------|---------|
| .NET Admin API | `Clavi.Sdk.Admin` |
| .NET ASP.NET Core OIDC | `Clavi.Sdk.AspNetCore` |
| JS | `@clavi/admin-client` |
| Java | `io.clavi:clavi-admin-client` |
| Python | `clavi-admin` |
| Go | `github.com/BelousovMike/clavi-sdk/go/clavi` |

### .NET Admin

```csharp
await using var client = new ClaviAdminClient(new ClaviClientOptions {
    BaseUrl = new Uri("https://clavi.example/")
});
await client.AuthAsync("admin", password);
var page = await client.Users.FindAsync(page: 1, pageSize: 20);
```

Or wire DI: `services.AddClaviAdmin(configuration.GetSection("Clavi"))` and inject `ClaviAdminClient`.

### .NET ASP.NET Core app (login users via Clavi)

Same configuration surface as Serilog — pick any mix:

```csharp
// 1) appsettings / env only (section "Clavi")
builder.Services.AddClaviOpenIdConnect();

// 2) bind a section explicitly
builder.Services.AddClaviOpenIdConnect(builder.Configuration.GetSection("Clavi"));

// 3) Options lambda
builder.Services.AddClaviOpenIdConnect(o =>
{
    o.Authority = "https://clavi.example";
    o.ClientId = "my-app";
});

// 4) Fluent (Serilog-style): config + overrides
builder.Services.AddClaviOpenIdConnect(c => c
    .ReadFrom.Configuration(builder.Configuration)
    .AcceptAnyServerCertificate(builder.Environment.IsDevelopment())
    .ClientId("my-app"));
```

```json
{
  "Clavi": {
    "Authority": "https://clavi.example",
    "ClientId": "my-app",
    "CallbackPath": "/signin-oidc"
  }
}
```

App policies still use roles from the Clavi token:

```csharp
builder.Services.AddAuthorization(o =>
    o.AddPolicy("catalog.write", p => p.RequireRole("acme.catalog.write")));
```


### JavaScript

```js
import ClaviAdminClient from '@clavi/admin-client';

const client = new ClaviAdminClient({ baseUrl: 'https://clavi.example/' });
await client.auth({ username: 'admin', password });
const page = await client.users.find({ page: 1, pageSize: 20 });
```

### Java

```java
try (var client = ClaviAdminClient.builder()
        .baseUrl("https://clavi.example/")
        .build()) {
    client.auth("admin", password);
    var page = client.users().find(1, 20, null);
}
```

### Python

```python
from clavi_admin import ClaviAdminClient

client = ClaviAdminClient(base_url="https://clavi.example/")
client.auth("admin", password)
page = client.users.find(page=1, page_size=20)
```

### Go

```go
c, err := clavi.New(clavi.Config{BaseURL: "https://clavi.example/"})
if err != nil { /* ... */ }
if err := c.Auth("admin", password); err != nil { /* ... */ }
page, err := c.Users.Find(1, 20, "")
```

OpenAPI pin used for codegen/docs: `openapi/openapi.v1.json` (refresh with `./scripts/refresh-openapi.sh`).
