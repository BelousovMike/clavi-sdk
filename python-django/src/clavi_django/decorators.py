from __future__ import annotations

from functools import wraps
from typing import Callable, Sequence

from django.http import HttpRequest, HttpResponse, JsonResponse


def clavi_login_required(view: Callable):
    @wraps(view)
    def wrapper(request: HttpRequest, *args, **kwargs):
        if not request.session.get("clavi_user"):
            return JsonResponse({"error": "unauthorized"}, status=401)
        return view(request, *args, **kwargs)

    return wrapper


def clavi_role_required(*roles: str | Sequence[str]):
    needed: list[str] = []
    for r in roles:
        if isinstance(r, str):
            needed.append(r)
        else:
            needed.extend(r)

    def decorator(view: Callable):
        @wraps(view)
        def wrapper(request: HttpRequest, *args, **kwargs):
            user = request.session.get("clavi_user")
            if not user:
                return JsonResponse({"error": "unauthorized"}, status=401)
            if needed and not any(role in (user.get("roles") or []) for role in needed):
                return JsonResponse({"error": "forbidden"}, status=403)
            return view(request, *args, **kwargs)

        return wrapper

    return decorator
