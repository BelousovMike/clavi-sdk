# clavi-django

```python
# settings.py
INSTALLED_APPS += ["clavi_django"]
CLAVI = {
    "Authority": "https://clavi.example",
    "ClientId": "my-app",
}

# urls.py
urlpatterns += [path("auth/", include("clavi_django.urls"))]
```

Views: `login/`, `signin-oidc/`, `logout/`, `signout-callback-oidc/`, `me/`.
Use `@clavi_login_required` / `@clavi_role_required("acme.catalog.write")`.
