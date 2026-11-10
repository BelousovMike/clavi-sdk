# clavi-admin

Admin API client for Clavi. Create a client, `auth`, then use `users` / `groups` / …

```python
from clavi_admin import ClaviAdminClient

client = ClaviAdminClient(base_url="https://clavi.example/")
client.auth("admin", password)
page = client.users.find(page=1, page_size=20)
```

`CLAVI_BASE_URL`, `CLAVI_TENANT_HOST`, and `CLAVI_ORIGIN` work if you omit constructor args.
For a self-signed local stack pass `accept_any_server_certificate=True`.
