# clavi-fastapi

```python
from fastapi import FastAPI, Depends
from clavi_fastapi import add_clavi_openid_connect, require_clavi_auth, get_clavi_admin

app = FastAPI()
add_clavi_openid_connect(app, lambda c: c.read_from_env().client_id("my-app"))

@app.get("/catalog/manage")
def manage(user=Depends(require_clavi_auth(["acme.catalog.write"]))):
    return {"user": user}
```
