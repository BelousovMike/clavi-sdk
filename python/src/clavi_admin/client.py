from __future__ import annotations

from .http import ClaviHttp
from .resources import (
    AuditResource,
    ClientsResource,
    GroupsResource,
    MeResource,
    RolesResource,
    SecurityResource,
    TenantsResource,
    UsersResource,
)


class ClaviAdminClient:
    """Admin API client.

    Create one instance, call :meth:`auth`, then use :attr:`users`, :attr:`groups`,
    and the other resources. The BFF session cookie stays on this object.

    Example::

        client = ClaviAdminClient(base_url="https://clavi.example/")
        client.auth("admin", password)
        page = client.users.find(page=1, page_size=20)
    """

    def __init__(
        self,
        base_url: str | None = None,
        *,
        tenant_host: str | None = None,
        origin: str | None = None,
        accept_any_server_certificate: bool = False,
    ) -> None:
        self._http = ClaviHttp(
            base_url or "",
            tenant_host=tenant_host,
            origin=origin,
            accept_any_server_certificate=accept_any_server_certificate,
        )
        self.users = UsersResource(self)
        self.groups = GroupsResource(self)
        self.roles = RolesResource(self)
        self.clients = ClientsResource(self)
        self.audit = AuditResource(self)
        self.tenants = TenantsResource(self)
        self.security = SecurityResource(self)
        self.me = MeResource(self)

    def auth(self, username: str, password: str) -> None:
        """Log in via the BFF; cookie is stored for later calls."""
        self._http.post("api/v1/auth/login", {"username": username, "password": password})

    def logout(self) -> None:
        """End the BFF session."""
        self._http.post("api/v1/auth/logout")
