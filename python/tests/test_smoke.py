import os

import pytest

from clavi_admin import ClaviAdminClient


def test_builder_requires_base_url(monkeypatch):
    monkeypatch.delenv("CLAVI_BASE_URL", raising=False)
    with pytest.raises(ValueError):
        ClaviAdminClient(base_url="")


@pytest.mark.skipif(os.environ.get("CLAVI_SDK_SMOKE") != "1", reason="set CLAVI_SDK_SMOKE=1")
def test_login_and_list_users():
    password = os.environ.get("CLAVI_E2E_ADMIN_PASSWORD")
    if not password:
        raise RuntimeError("CLAVI_E2E_ADMIN_PASSWORD required for smoke")
    username = os.environ.get("CLAVI_E2E_ADMIN_USERNAME", "admin")
    base_url = os.environ.get("CLAVI_BASE_URL", "https://clavi.clavi.local/")
    host = os.environ.get("CLAVI_TENANT_HOST", "clavi.clavi.local")

    client = ClaviAdminClient(
        base_url=base_url,
        tenant_host=host,
        accept_any_server_certificate=True,
    )
    client.auth(username, password)
    page = client.users.find(page=1, page_size=10)
    assert page is not None
    assert "items" in page

