package io.clavi.admin.resource;

import io.clavi.admin.ClaviHttp;
import io.clavi.admin.model.PasswordPolicyResponse;

public final class SecurityResource {
    private final ClaviHttp http;

    public SecurityResource(ClaviHttp http) {
        this.http = http;
    }

    public PasswordPolicyResponse getPasswordPolicy() {
        return http.get("api/v1/security/policies/password", PasswordPolicyResponse.class);
    }
}
