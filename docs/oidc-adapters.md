# Clavi OIDC + Admin adapters

Shared contract for framework packages that wire **app login** (OIDC RP) and the **Admin API client**.

Reference implementation: `Clavi.Sdk.AspNetCore` (`AddClaviOpenIdConnect` + `AddClaviAdmin`).

## Config section

Name: **`Clavi`**. Environment variables use framework idioms:

| Ecosystem | Example |
|-----------|---------|
| .NET / Java Spring | `Clavi__Authority`, `Clavi__ClientId` |
| Node | `CLAVI_AUTHORITY`, `CLAVI_CLIENT_ID` (also nested JSON `Clavi`) |
| Python | `CLAVI_AUTHORITY` / settings dict `CLAVI` |
| Go | `CLAVI_AUTHORITY` |

```yaml
Clavi:
  # OIDC (relying party)
  Authority: https://clavi.example
  ClientId: my-app
  ClientSecret:                 # optional (public + PKCE leave empty)
  CallbackPath: /signin-oidc
  SignedOutCallbackPath: /signout-callback-oidc
  Scopes: [openid, profile, email]
  MapRoles: true
  AcceptAnyServerCertificate: false   # lab / self-signed only
  DisablePushedAuthorization: true    # when public URL differs from container Host

  # Admin API
  BaseUrl: https://clavi.example/     # may equal Authority
  TenantHost:                         # optional Host override
```

### Defaults

| Field | Default |
|-------|---------|
| CallbackPath | `/signin-oidc` |
| SignedOutCallbackPath | `/signout-callback-oidc` |
| Scopes | `openid`, `profile`, `email` |
| MapRoles | `true` |
| AcceptAnyServerCertificate | `false` |
| DisablePushedAuthorization | `true` |

### Validation

- OIDC: `Authority` and `ClientId` required.
- Admin: `BaseUrl` required (adapters may fall back to `Authority` when `BaseUrl` is empty).

## OIDC behavior

Every adapter must:

1. Use Authorization Code + **PKCE**.
2. Persist the signed-in user in a cookie/session (or framework-native session).
3. Expose login, callback, and logout (paths configurable; defaults above).
4. When `MapRoles` is true, map Clavi `roles` (JSON array on userinfo and/or access token) into the framework role model.
5. Accept configuration from: config file / env, imperative options, and a fluent/`ReadFrom`-style builder where idiomatic.

## Role mapping

Clavi puts `roles: string[]` on userinfo and often on the access token.

Adapters copy each non-empty role string into:

| Framework | Target |
|-----------|--------|
| ASP.NET Core | `ClaimTypes.Role` |
| Express / Nest | `req.user.roles` (string[]) |
| NextAuth | `session.user.roles` |
| FastAPI / Django | `user.roles` / request state |
| Spring | `ROLE_*` or granted authorities named as Clavi roles (no forced `ROLE_` prefix — use Clavi names as-is) |
| Go | `Principal.Roles` |

## Admin client

From the same `Clavi` section, adapters construct the language’s Admin client (`ClaviAdminClient` / `clavi.New` / …) and register it in DI / app state.

Admin login (`auth` / cookie session against Clavi’s browser session API) stays separate from end-user OIDC login. There is **no** `@clavi/sdk-bff` package — that alias was removed.

## Package matrix (Wave 1)

| Package | Role |
|---------|------|
| `Clavi.Sdk.AspNetCore` | .NET OIDC |
| `Clavi.Sdk.Admin` | .NET Admin + DI |
| `@clavi/oidc` | Shared JS options / PKCE / roles |
| `@clavi/express` | Express middleware + Admin |
| `@clavi/nestjs` | NestJS module |
| `@clavi/next` | Auth.js / NextAuth provider |
| `clavi-oidc` | Shared Python options / Authlib helpers |
| `clavi-fastapi` | FastAPI router + Depends |
| `clavi-django` | Django views + settings |
| `io.clavi:clavi-spring-boot-starter` | Spring Boot OAuth2 + Admin bean |
| `…/go/clavi/oidc` | `net/http` middleware |
