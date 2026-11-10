# clavi-spring-boot-starter

```yaml
clavi:
  authority: https://clavi.example
  client-id: my-app
  base-url: https://clavi.example/
```

```xml
<dependency>
  <groupId>io.clavi</groupId>
  <artifactId>clavi-spring-boot-starter</artifactId>
  <version>0.1.0</version>
</dependency>
```

Auto-configures OAuth2 login (registration id `clavi`), role mapping from Clavi `roles`, and a `ClaviAdminClient` bean.
