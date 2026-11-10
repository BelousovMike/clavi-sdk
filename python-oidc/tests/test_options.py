from clavi_oidc import ClaviOidcConfiguration, create_oidc_options, roles_from_claims, resolve_admin_base_url


def test_fluent_and_defaults():
    opts = (
        ClaviOidcConfiguration()
        .read_from_mapping({"Authority": "https://clavi.example/", "ClientId": "app"})
        .accept_any_server_certificate(True)
        .build()
    )
    assert opts.authority == "https://clavi.example"
    assert opts.callback_path == "/signin-oidc"
    assert resolve_admin_base_url(opts) == "https://clavi.example/"


def test_roles():
    # minimal unsigned JWT payload
    import base64
    import json

    payload = base64.urlsafe_b64encode(json.dumps({"roles": ["a", "b"]}).encode()).rstrip(b"=").decode()
    token = f"x.{payload}.y"
    roles = roles_from_claims(user_info={"roles": ["b", "c"]}, access_token=token, map_roles=True)
    assert roles == ["a", "b", "c"]
