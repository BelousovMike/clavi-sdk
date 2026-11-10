package io.clavi.admin.resource;

import com.fasterxml.jackson.core.type.TypeReference;
import io.clavi.admin.ClaviHttp;
import io.clavi.admin.model.ClientResponse;
import io.clavi.admin.model.PagedResult;

import java.util.UUID;

public final class ClientsResource {
    private final ClaviHttp http;

    public ClientsResource(ClaviHttp http) {
        this.http = http;
    }

    public PagedResult<ClientResponse> find(int page, int pageSize, String search) {
        return http.get(
                ClaviHttp.pagedPath("api/v1/oidc/clients", page, pageSize, search),
                new TypeReference<PagedResult<ClientResponse>>() {
                });
    }

    public ClientResponse findOne(UUID id) {
        return http.get("api/v1/oidc/clients/" + id, ClientResponse.class);
    }
}
