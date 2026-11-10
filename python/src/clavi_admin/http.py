from __future__ import annotations

import json
import os
import ssl
import urllib.error
import urllib.parse
import urllib.request
from typing import Any
from urllib.request import Request

from .errors import ClaviApiError


class ClaviHttp:
    def __init__(
        self,
        base_url: str,
        *,
        tenant_host: str | None = None,
        origin: str | None = None,
        accept_any_server_certificate: bool = False,
    ) -> None:
        base = (base_url or os.environ.get("CLAVI_BASE_URL") or "").rstrip("/") + "/"
        if base == "/":
            raise ValueError("base_url / CLAVI_BASE_URL is required")
        parsed = urllib.parse.urlparse(base)
        self.base_url = base
        self.tenant_host = tenant_host or os.environ.get("CLAVI_TENANT_HOST") or parsed.hostname or ""
        self.origin = origin or os.environ.get("CLAVI_ORIGIN") or f"{parsed.scheme}://{parsed.netloc}"
        self._cookies: dict[str, str] = {}
        self._ssl = None
        if accept_any_server_certificate:
            ctx = ssl.create_default_context()
            ctx.check_hostname = False
            ctx.verify_mode = ssl.CERT_NONE
            self._ssl = ctx

    def get(self, path: str) -> Any:
        return self.request("GET", path)

    def post(self, path: str, body: dict | None = None) -> Any:
        return self.request("POST", path, body)

    def delete(self, path: str) -> Any:
        return self.request("DELETE", path)

    def request(self, method: str, path: str, body: dict | None = None) -> Any:
        url = urllib.parse.urljoin(self.base_url, path.lstrip("/"))
        data = None if body is None else json.dumps(body).encode("utf-8")
        headers = {
            "Accept": "application/json",
            "Origin": self.origin,
        }
        host = urllib.parse.urlparse(url).hostname
        if self.tenant_host and host and self.tenant_host.lower() != host.lower():
            headers["Host"] = self.tenant_host
        if self._cookies:
            headers["Cookie"] = "; ".join(f"{k}={v}" for k, v in self._cookies.items())
        if body is not None:
            headers["Content-Type"] = "application/json"

        req = Request(url, data=data, headers=headers, method=method)
        try:
            with urllib.request.urlopen(req, context=self._ssl, timeout=60) as resp:
                self._store_cookies(resp.headers.get_all("Set-Cookie") or [])
                raw = resp.read().decode("utf-8")
                if resp.status == 204 or not raw:
                    return None
                return json.loads(raw)
        except urllib.error.HTTPError as e:
            raw = e.read().decode("utf-8", errors="replace")
            lines: list[str] = []
            if e.headers is not None:
                lines = e.headers.get_all("Set-Cookie") or []
            self._store_cookies(lines)
            code = None
            try:
                payload = json.loads(raw)
                code = payload.get("code") or (payload.get("extensions") or {}).get("code")
            except json.JSONDecodeError:
                pass
            raise ClaviApiError(e.code, code, raw) from e

    def _store_cookies(self, lines: list[str]) -> None:
        for line in lines or []:
            pair = line.split(";", 1)[0]
            if "=" in pair:
                k, v = pair.split("=", 1)
                self._cookies[k.strip()] = v.strip()

    @staticmethod
    def paged_path(path: str, page: int = 1, page_size: int = 20, search: str | None = None) -> str:
        q = {"page": str(page), "page_size": str(page_size)}
        if search and search.strip():
            q["search"] = search.strip()
        return f"{path}?{urllib.parse.urlencode(q)}"
