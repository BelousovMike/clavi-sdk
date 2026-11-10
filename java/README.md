# Java SDK

Maven multi-module (`java/pom.xml`):

| Artifact | Role |
|----------|------|
| `io.clavi:clavi-admin-client` | Admin API client |
| `io.clavi:clavi-spring-boot-starter` | OAuth2 login + `ClaviAdminClient` bean (`clavi.*` properties) |

```bash
mvn -B test
mvn -B -DskipTests package
```

Config example for the starter — see `spring-boot-starter/README.md` and `../docs/oidc-adapters.md`.
