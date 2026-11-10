from __future__ import annotations

import secrets
from urllib.parse import urljoin

from clavi_oidc import (
    build_authorize_url,
    create_pkce_pair,
    discovery,
    end_session_url,
    exchange_code,
    fetch_userinfo,
    roles_from_claims,
)
from django.http import HttpRequest, HttpResponse, HttpResponseBadRequest, JsonResponse
from django.shortcuts import redirect
from django.views.decorators.http import require_GET

from .settings import get_clavi_options


def _abs(request: HttpRequest, path: str) -> str:
    return urljoin(request.build_absolute_uri("/"), path.lstrip("/"))


def _verify(opts) -> bool:
    return not opts.accept_any_server_certificate


@require_GET
def login(request: HttpRequest) -> HttpResponse:
    opts = get_clavi_options()
    meta = discovery(opts.authority, verify=_verify(opts))
    pkce = create_pkce_pair()
    state = secrets.token_urlsafe(16)
    nonce = secrets.token_urlsafe(16)
    request.session["clavi_oidc"] = {
        "verifier": pkce["verifier"],
        "state": state,
        "nonce": nonce,
        "return_to": request.GET.get("return_to") or "/",
    }
    url = build_authorize_url(
        authorization_endpoint=meta["authorization_endpoint"],
        client_id=opts.client_id,
        redirect_uri=_abs(request, opts.callback_path),
        scope=" ".join(opts.scopes),
        state=state,
        nonce=nonce,
        code_challenge=pkce["challenge"],
    )
    return redirect(url)


@require_GET
def callback(request: HttpRequest) -> HttpResponse:
    opts = get_clavi_options()
    pending = request.session.get("clavi_oidc")
    if not pending:
        return HttpResponseBadRequest("missing OIDC session")
    if request.GET.get("error"):
        return HttpResponseBadRequest(request.GET.get("error_description") or request.GET["error"])
    if request.GET.get("state") != pending["state"]:
        return HttpResponseBadRequest("invalid state")
    meta = discovery(opts.authority, verify=_verify(opts))
    tokens = exchange_code(
        token_endpoint=meta["token_endpoint"],
        client_id=opts.client_id,
        client_secret=opts.client_secret,
        code=request.GET.get("code") or "",
        redirect_uri=_abs(request, opts.callback_path),
        code_verifier=pending["verifier"],
        verify=_verify(opts),
    )
    user_info = {}
    if meta.get("userinfo_endpoint"):
        user_info = fetch_userinfo(meta["userinfo_endpoint"], tokens["access_token"], verify=_verify(opts))
    roles = roles_from_claims(
        user_info=user_info,
        access_token=tokens.get("access_token"),
        map_roles=opts.map_roles,
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
    return redirect(pending.get("return_to") or "/")


@require_GET
def logout(request: HttpRequest) -> HttpResponse:
    opts = get_clavi_options()
    user = request.session.get("clavi_user") or {}
    meta = discovery(opts.authority, verify=_verify(opts))
    post = _abs(request, opts.signed_out_callback_path)
    request.session.pop("clavi_user", None)
    request.session.pop("clavi_oidc", None)
    url = end_session_url(
        meta.get("end_session_endpoint"),
        id_token=user.get("id_token"),
        post_logout_redirect_uri=post,
    )
    return redirect(url or opts.signed_out_callback_path)


@require_GET
def signed_out(request: HttpRequest) -> HttpResponse:
    return redirect("/")


@require_GET
def me(request: HttpRequest) -> HttpResponse:
    user = request.session.get("clavi_user")
    if not user:
        return JsonResponse({"authenticated": False}, status=401)
    safe = {k: v for k, v in user.items() if k not in ("id_token", "access_token")}
    return JsonResponse({"authenticated": True, "user": safe})
