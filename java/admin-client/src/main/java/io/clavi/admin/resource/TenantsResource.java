package io.clavi.admin.resource;

import com.fasterxml.jackson.core.type.TypeReference;
import io.clavi.admin.ClaviHttp;
import io.clavi.admin.model.PagedResult;
import io.clavi.admin.model.TenantResponse;

import java.util.UUID;

public final class TenantsResource {
    private final ClaviHttp http;

    public TenantsResource(ClaviHttp http) {
        this.http = http;
    }

    public PagedResult<TenantResponse> find(int page, int pageSize) {
        return http.get(
                ClaviHttp.pagedPath("api/v1/system/tenants", page, pageSize, null),
                new TypeReference<PagedResult<TenantResponse>>() {
                });
    }

    public TenantResponse findOne(UUID id) {
        return http.get("api/v1/system/tenants/" + id, TenantResponse.class);
    }
}
