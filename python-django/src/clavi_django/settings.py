from __future__ import annotations

from functools import lru_cache

from clavi_admin import ClaviAdminClient
from clavi_oidc import ClaviOidcConfiguration, ClaviOidcOptions, resolve_admin_base_url
from django.conf import settings


@lru_cache
def get_clavi_options() -> ClaviOidcOptions:
    raw = getattr(settings, "CLAVI", None) or {}
    return ClaviOidcConfiguration().read_from_env().read_from_mapping(raw).build()


@lru_cache
def get_clavi_admin() -> ClaviAdminClient:
    opts = get_clavi_options()
    return ClaviAdminClient(
        base_url=resolve_admin_base_url(opts),
        tenant_host=opts.tenant_host,
    )
