# clavi-oidc

Shared Clavi OIDC configuration (Serilog-style) and Authlib helpers.

```python
from clavi_oidc import ClaviOidcConfiguration, create_oidc_options

opts = (
    ClaviOidcConfiguration()
    .read_from_env()
    .client_id("my-app")
    .accept_any_server_certificate(True)
    .build()
)
# or: create_oidc_options(authority="https://clavi.example", client_id="my-app")
```
