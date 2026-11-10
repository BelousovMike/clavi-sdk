from __future__ import annotations

import base64
import hashlib
import json
import secrets
from typing import Any, Mapping
from urllib.parse import urlencode

import httpx


def create_pkce_pair(nbytes: int = 32) -> dict[str, str]:
    verifier = _b64url(secrets.token_bytes(nbytes))
    challenge = _b64url(hashlib.sha256(verifier.encode("ascii")).digest())
    return {"verifier": verifier, "challenge": challenge, "method": "S256"}


def _b64url(data: bytes | str) -> str:
    if isinstance(data, str):
        data = data.encode("ascii")
    return base64.urlsafe_b64encode(data).rstrip(b"=").decode("ascii")


def discovery(authority: str, *, verify: bool = True) -> dict[str, Any]:
    base = authority.rstrip("/")
    url = f"{base}/.well-known/openid-configuration"
    with httpx.Client(verify=verify, timeout=30.0) as client:
        res = client.get(url)
        res.raise_for_status()
        return res.json()


def build_authorize_url(
    *,
    authorization_endpoint: str,
    client_id: str,
    redirect_uri: str,
    scope: str,
    state: str,
    nonce: str,
    code_challenge: str,
    code_challenge_method: str = "S256",
) -> str:
    q = urlencode(
        {
            "client_id": client_id,
            "response_type": "code",
            "redirect_uri": redirect_uri,
            "scope": scope,
            "state": state,
            "nonce": nonce,
            "code_challenge": code_challenge,
            "code_challenge_method": code_challenge_method,
        }
    )
    sep = "&" if "?" in authorization_endpoint else "?"
    return f"{authorization_endpoint}{sep}{q}"


def exchange_code(
    *,
    token_endpoint: str,
    client_id: str,
    code: str,
    redirect_uri: str,
    code_verifier: str,
    client_secret: str | None = None,
    verify: bool = True,
) -> dict[str, Any]:
    data = {
        "grant_type": "authorization_code",
        "client_id": client_id,
        "code": code,
        "redirect_uri": redirect_uri,
        "code_verifier": code_verifier,
    }
    if client_secret:
        data["client_secret"] = client_secret
    with httpx.Client(verify=verify, timeout=30.0) as client:
        res = client.post(token_endpoint, data=data)
        res.raise_for_status()
        return res.json()


def fetch_userinfo(userinfo_endpoint: str, access_token: str, *, verify: bool = True) -> dict[str, Any]:
    with httpx.Client(verify=verify, timeout=30.0) as client:
        res = client.get(userinfo_endpoint, headers={"Authorization": f"Bearer {access_token}"})
        res.raise_for_status()
        return res.json()


def end_session_url(
    end_session_endpoint: str | None,
    *,
    id_token: str | None = None,
    post_logout_redirect_uri: str | None = None,
) -> str | None:
    if not end_session_endpoint:
        return None
    params: dict[str, str] = {}
    if id_token:
        params["id_token_hint"] = id_token
    if post_logout_redirect_uri:
        params["post_logout_redirect_uri"] = post_logout_redirect_uri
    if not params:
        return end_session_endpoint
    return f"{end_session_endpoint}?{urlencode(params)}"


def decode_jwt_payload(jwt: str | None) -> dict[str, Any] | None:
    if not jwt or not isinstance(jwt, str):
        return None
    parts = jwt.split(".")
    if len(parts) < 2:
        return None
    try:
        padded = parts[1] + "=" * (-len(parts[1]) % 4)
        raw = base64.urlsafe_b64decode(padded.encode("ascii"))
        return json.loads(raw.decode("utf-8"))
    except (ValueError, json.JSONDecodeError):
        return None


def roles_from_claims(
    *,
    user_info: Mapping[str, Any] | None = None,
    access_token: str | None = None,
    map_roles: bool = True,
) -> list[str]:
    if not map_roles:
        return []
    roles: set[str] = set()

    def add(arr: Any) -> None:
        if isinstance(arr, list):
            for item in arr:
                if isinstance(item, str) and item.strip():
                    roles.add(item.strip())

    if user_info:
        add(user_info.get("roles"))
    add((decode_jwt_payload(access_token) or {}).get("roles"))
    return sorted(roles)
