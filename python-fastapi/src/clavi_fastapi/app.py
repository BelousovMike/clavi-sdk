from __future__ import annotations

import secrets
from typing import Any, Callable, Sequence
from urllib.parse import urljoin

from clavi_admin import ClaviAdminClient
from clavi_oidc import (
    ClaviOidcConfiguration,
    ClaviOidcOptions,
    build_authorize_url,
    create_oidc_options,
    create_pkce_pair,
    discovery,
    end_session_url,
    exchange_code,
    fetch_userinfo,
    resolve_admin_base_url,
    roles_from_claims,
)
from fastapi import Depends, FastAPI, HTTPException, Request
from fastapi.responses import RedirectResponse
from starlette.middleware.sessions import SessionMiddleware


Configure = ClaviOidcOptions | Callable[[ClaviOidcConfiguration], Any] | dict[str, Any] | None


def add_clavi_openid_connect(
    app: FastAPI,
    configure: Configure = None,
    *,
    session_secret: str | None = None,
    public_origin: str | None = None,
    login_path: str = "/login",
    logout_path: str = "/logout",
    me_path: str = "/me",
) -> ClaviOidcOptions:
    options = _resolve(configure)
    verify = not options.accept_any_server_certificate

    if not any(isinstance(m, SessionMiddleware) for m in app.user_middleware):
        app.add_middleware(
            SessionMiddleware,
            secret_key=session_secret or secrets.token_urlsafe(32),
            same_site="lax",
        )

    admin = ClaviAdminClient(
        base_url=resolve_admin_base_url(options),
        tenant_host=options.tenant_host,
    )
    app.state.clavi_oidc = options
    app.state.clavi_admin = admin

    meta_cache: dict[str, Any] = {}

    def get_meta() -> dict[str, Any]:
        if "doc" not in meta_cache:
            meta_cache["doc"] = discovery(options.authority, verify=verify)
        return meta_cache["doc"]

    def abs_url(request: Request, path: str) -> str:
        origin = public_origin or str(request.base_url).rstrip("/")
        return urljoin(origin + "/", path.lstrip("/"))

    @app.get(login_path)
    async def login(request: Request, return_to: str = "/") -> RedirectResponse:
        meta = get_meta()
        pkce = create_pkce_pair()
        state = secrets.token_urlsafe(16)
        nonce = secrets.token_urlsafe(16)
        request.session["clavi_oidc"] = {
            "verifier": pkce["verifier"],
            "state": state,
            "nonce": nonce,
            "return_to": return_to,
        }
        url = build_authorize_url(
            authorization_endpoint=meta["authorization_endpoint"],
            client_id=options.client_id,
            redirect_uri=abs_url(request, options.callback_path),
            scope=" ".join(options.scopes),
            state=state,
            nonce=nonce,
            code_challenge=pkce["challenge"],
        )
        return RedirectResponse(url)

    @app.get(options.callback_path)
    async def callback(request: Request) -> RedirectResponse:
        pending = request.session.get("clavi_oidc")
        if not pending:
            raise HTTPException(400, "missing OIDC session")
        if request.query_params.get("error"):
            raise HTTPException(400, request.query_params.get("error_description") or request.query_params["error"])
        if request.query_params.get("state") != pending["state"]:
            raise HTTPException(400, "invalid state")
        meta = get_meta()
        tokens = exchange_code(
            token_endpoint=meta["token_endpoint"],
            client_id=options.client_id,
            client_secret=options.client_secret,
            code=request.query_params.get("code") or "",
            redirect_uri=abs_url(request, options.callback_path),
            code_verifier=pending["verifier"],
            verify=verify,
        )
        user_info = {}
        if meta.get("userinfo_endpoint"):
            user_info = fetch_userinfo(meta["userinfo_endpoint"], tokens["access_token"], verify=verify)
        roles = roles_from_claims(
            user_info=user_info,
            access_token=tokens.get("access_token"),
            map_roles=options.map_roles,
        )
        request.session["clavi_user"] = {
            "sub": user_info.get("sub"),
            "name": user_info.get("preferred_username") or user_info.get("name") or user_info.get("email"),
            "email": user_info.get("email"),
            "roles": roles,
            "id_token": tokens.get("id_token"),
            "access_token": tokens.get("access_token"),
        }
        request.session.pop("clavi_oidc", None)
        return RedirectResponse(pending.get("return_to") or "/")

    @app.get(logout_path)
    async def logout(request: Request) -> RedirectResponse:
        user = request.session.get("clavi_user") or {}
        meta = get_meta()
        post = abs_url(request, options.signed_out_callback_path)
        request.session.pop("clavi_user", None)
        request.session.pop("clavi_oidc", None)
        url = end_session_url(
            meta.get("end_session_endpoint"),
            id_token=user.get("id_token"),
            post_logout_redirect_uri=post,
        )
        return RedirectResponse(url or options.signed_out_callback_path)

    @app.get(options.signed_out_callback_path)
    async def signed_out() -> RedirectResponse:
        return RedirectResponse("/")

    @app.get(me_path)
    async def me(request: Request) -> dict[str, Any]:
        user = request.session.get("clavi_user")
        if not user:
            raise HTTPException(401, "unauthorized")
        safe = {k: v for k, v in user.items() if k not in ("id_token", "access_token")}
        return {"authenticated": True, "user": safe}

    return options


def get_clavi_admin(request: Request) -> ClaviAdminClient:
    admin = getattr(request.app.state, "clavi_admin", None)
    if admin is None:
        raise RuntimeError("call add_clavi_openid_connect first")
    return admin


def require_clavi_auth(roles: Sequence[str] | str | None = None):
    needed = [] if roles is None else ([roles] if isinstance(roles, str) else list(roles))

    def dependency(request: Request) -> dict[str, Any]:
        user = request.session.get("clavi_user")
        if not user:
            raise HTTPException(401, "unauthorized")
        if needed and not any(r in (user.get("roles") or []) for r in needed):
            raise HTTPException(403, "forbidden")
        return user

    return dependency


def _resolve(configure: Configure) -> ClaviOidcOptions:
    if configure is None:
        return create_oidc_options()
    if callable(configure):
        c = ClaviOidcConfiguration()
        configure(c)
        return c.build()
    if isinstance(configure, ClaviOidcOptions):
        configure.validate()
        return configure
    if isinstance(configure, dict):
        if "authority" in configure or "client_id" in configure:
            return create_oidc_options(**configure)
        return ClaviOidcConfiguration().read_from_mapping(configure).build()
    raise TypeError("unsupported configure argument")
