from __future__ import annotations

import os
from dataclasses import dataclass, field, replace
from typing import Any, Callable, Iterable, Mapping, Sequence


def _env(*names: str) -> str | None:
    for name in names:
        value = os.environ.get(name)
        if value:
            return value
    return None


@dataclass
class ClaviOidcOptions:
    authority: str
    client_id: str
    client_secret: str | None = None
    callback_path: str = "/signin-oidc"
    signed_out_callback_path: str = "/signout-callback-oidc"
    scopes: Sequence[str] = field(default_factory=lambda: ["openid", "profile", "email"])
    map_roles: bool = True
    accept_any_server_certificate: bool = False
    disable_pushed_authorization: bool = True
    base_url: str | None = None
    tenant_host: str | None = None

    def validate(self) -> None:
        if not self.authority or not self.authority.strip():
            raise ValueError("Clavi:Authority / CLAVI_AUTHORITY is required")
        if not self.client_id or not self.client_id.strip():
            raise ValueError("Clavi:ClientId / CLAVI_CLIENT_ID is required")


def create_oidc_options(**partial: Any) -> ClaviOidcOptions:
    scopes_raw = _env("CLAVI_SCOPES", "Clavi__Scopes")
    scopes = partial.get("scopes")
    if scopes is None and scopes_raw:
        scopes = [s for s in scopes_raw.replace(",", " ").split() if s]
    if scopes is None:
        scopes = ["openid", "profile", "email"]

    opts = ClaviOidcOptions(
        authority=(partial.get("authority") or _env("CLAVI_AUTHORITY", "Clavi__Authority") or "").rstrip("/"),
        client_id=partial.get("client_id") or _env("CLAVI_CLIENT_ID", "Clavi__ClientId") or "",
        client_secret=partial.get("client_secret") or _env("CLAVI_CLIENT_SECRET", "Clavi__ClientSecret"),
        callback_path=partial.get("callback_path")
        or _env("CLAVI_CALLBACK_PATH", "Clavi__CallbackPath")
        or "/signin-oidc",
        signed_out_callback_path=partial.get("signed_out_callback_path")
        or _env("CLAVI_SIGNED_OUT_CALLBACK_PATH", "Clavi__SignedOutCallbackPath")
        or "/signout-callback-oidc",
        scopes=list(scopes),
        map_roles=partial["map_roles"] if "map_roles" in partial else True,
        accept_any_server_certificate=partial.get("accept_any_server_certificate", False),
        disable_pushed_authorization=partial.get("disable_pushed_authorization", True),
        base_url=partial.get("base_url") or _env("CLAVI_BASE_URL", "Clavi__BaseUrl"),
        tenant_host=partial.get("tenant_host") or _env("CLAVI_TENANT_HOST", "Clavi__TenantHost"),
    )
    opts.validate()
    return opts


def resolve_admin_base_url(opts: ClaviOidcOptions) -> str:
    raw = (opts.base_url or opts.authority or "").rstrip("/") + "/"
    if raw == "/":
        raise ValueError("Clavi:BaseUrl or Authority is required for Admin")
    return raw


_PASCAL = {
    "Authority": "authority",
    "ClientId": "client_id",
    "ClientSecret": "client_secret",
    "CallbackPath": "callback_path",
    "SignedOutCallbackPath": "signed_out_callback_path",
    "Scopes": "scopes",
    "MapRoles": "map_roles",
    "AcceptAnyServerCertificate": "accept_any_server_certificate",
    "DisablePushedAuthorization": "disable_pushed_authorization",
    "BaseUrl": "base_url",
    "TenantHost": "tenant_host",
}


class ClaviOidcConfiguration:
    """Fluent builder: ``ClaviOidcConfiguration().read_from_env().client_id('x').build()``."""

    def __init__(self) -> None:
        self._partial: dict[str, Any] = {}

    def read_from_env(self) -> ClaviOidcConfiguration:
        return self

    def read_from_mapping(self, obj: Mapping[str, Any]) -> ClaviOidcConfiguration:
        section: Mapping[str, Any] = obj.get("Clavi", obj) if isinstance(obj.get("Clavi"), Mapping) else obj
        for key, value in section.items():
            dest = _PASCAL.get(key, key)
            if value is not None and value != "":
                self._partial[dest] = value
        return self

    def authority(self, v: str) -> ClaviOidcConfiguration:
        self._partial["authority"] = v
        return self

    def client_id(self, v: str) -> ClaviOidcConfiguration:
        self._partial["client_id"] = v
        return self

    def client_secret(self, v: str | None) -> ClaviOidcConfiguration:
        self._partial["client_secret"] = v
        return self

    def callback_path(self, v: str) -> ClaviOidcConfiguration:
        self._partial["callback_path"] = v
        return self

    def signed_out_callback_path(self, v: str) -> ClaviOidcConfiguration:
        self._partial["signed_out_callback_path"] = v
        return self

    def scopes(self, *scopes: str | Iterable[str]) -> ClaviOidcConfiguration:
        flat: list[str] = []
        for s in scopes:
            if isinstance(s, str):
                flat.append(s)
            else:
                flat.extend(s)
        self._partial["scopes"] = flat
        return self

    def map_roles(self, enabled: bool = True) -> ClaviOidcConfiguration:
        self._partial["map_roles"] = enabled
        return self

    def accept_any_server_certificate(self, accept: bool = True) -> ClaviOidcConfiguration:
        self._partial["accept_any_server_certificate"] = accept
        return self

    def base_url(self, v: str) -> ClaviOidcConfiguration:
        self._partial["base_url"] = v
        return self

    def tenant_host(self, v: str) -> ClaviOidcConfiguration:
        self._partial["tenant_host"] = v
        return self

    def configure(self, fn: Callable[[dict[str, Any]], None]) -> ClaviOidcConfiguration:
        fn(self._partial)
        return self

    def build(self) -> ClaviOidcOptions:
        return create_oidc_options(**self._partial)
