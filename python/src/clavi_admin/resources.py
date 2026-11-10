from __future__ import annotations

from typing import TYPE_CHECKING, Any

from .http import ClaviHttp

if TYPE_CHECKING:
    from .client import ClaviAdminClient


class _Resource:
    def __init__(self, client: ClaviAdminClient) -> None:
        self._http: ClaviHttp = client._http


class UsersResource(_Resource):
    """`/api/v1/users`."""

    def find(self, page: int = 1, page_size: int = 20, search: str | None = None) -> dict[str, Any]:
        """Paged list; optional search."""
        return self._http.get(ClaviHttp.paged_path("api/v1/users", page, page_size, search))

    def find_one(self, id: str) -> dict[str, Any]:
        return self._http.get(f"api/v1/users/{id}")

    def create(self, username: str, password: str, email: str | None = None) -> dict[str, Any]:
        body: dict[str, Any] = {"username": username, "password": password}
        if email is not None:
            body["email"] = email
        return self._http.post("api/v1/users", body)

    def delete(self, id: str) -> None:
        self._http.delete(f"api/v1/users/{id}")


class GroupsResource(_Resource):
    """`/api/v1/groups`."""

    def find(self, page: int = 1, page_size: int = 20, search: str | None = None) -> dict[str, Any]:
        return self._http.get(ClaviHttp.paged_path("api/v1/groups", page, page_size, search))

    def create(self, name: str, description: str | None = None, parent_id: str | None = None) -> dict[str, Any]:
        body: dict[str, Any] = {"name": name}
        if description is not None:
            body["description"] = description
        if parent_id is not None:
            body["parentId"] = parent_id
        return self._http.post("api/v1/groups", body)

    def delete(self, id: str) -> None:
        self._http.delete(f"api/v1/groups/{id}")


class RolesResource(_Resource):
    """`/api/v1/roles`."""

    def find(self) -> list[Any]:
        return self._http.get("api/v1/roles")


class ClientsResource(_Resource):
    """OIDC clients (`/api/v1/oidc/clients`)."""

    def find(self, page: int = 1, page_size: int = 20, search: str | None = None) -> dict[str, Any]:
        return self._http.get(ClaviHttp.paged_path("api/v1/oidc/clients", page, page_size, search))


class AuditResource(_Resource):
    """Audit entries."""

    def find(self, page: int = 1, page_size: int = 20) -> dict[str, Any]:
        return self._http.get(ClaviHttp.paged_path("api/v1/audit/entries", page, page_size))


class TenantsResource(_Resource):
    """System tenants (platform admin)."""

    def find(self, page: int = 1, page_size: int = 20) -> dict[str, Any]:
        return self._http.get(ClaviHttp.paged_path("api/v1/system/tenants", page, page_size))


class SecurityResource(_Resource):
    """Tenant security policies."""

    def get_password_policy(self) -> dict[str, Any]:
        return self._http.get("api/v1/security/policies/password")


class MeResource(_Resource):
    """Current session profile."""

    def get(self) -> dict[str, Any]:
        return self._http.get("api/v1/me")

    def list_sessions(self, page: int = 1, page_size: int = 20) -> dict[str, Any]:
        return self._http.get(ClaviHttp.paged_path("api/v1/me/sessions", page, page_size))
