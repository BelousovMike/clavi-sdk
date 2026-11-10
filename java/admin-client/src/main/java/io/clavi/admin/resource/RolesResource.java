package io.clavi.admin.resource;

import com.fasterxml.jackson.core.type.TypeReference;
import io.clavi.admin.ClaviHttp;
import io.clavi.admin.model.RoleResponse;

import java.util.List;

public final class RolesResource {
    private final ClaviHttp http;

    public RolesResource(ClaviHttp http) {
        this.http = http;
    }

    public List<RoleResponse> find() {
        return http.get("api/v1/roles", new TypeReference<List<RoleResponse>>() {
        });
    }
}
