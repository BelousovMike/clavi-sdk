package io.clavi.admin.resource;

import com.fasterxml.jackson.core.type.TypeReference;
import io.clavi.admin.ClaviHttp;
import io.clavi.admin.model.CreateGroupRequest;
import io.clavi.admin.model.GroupResponse;
import io.clavi.admin.model.PagedResult;

import java.util.UUID;

public final class GroupsResource {
    private final ClaviHttp http;

    public GroupsResource(ClaviHttp http) {
        this.http = http;
    }

    public PagedResult<GroupResponse> find(int page, int pageSize, String search) {
        return http.get(
                ClaviHttp.pagedPath("api/v1/groups", page, pageSize, search),
                new TypeReference<PagedResult<GroupResponse>>() {
                });
    }

    public GroupResponse findOne(UUID id) {
        return http.get("api/v1/groups/" + id, GroupResponse.class);
    }

    public GroupResponse create(CreateGroupRequest request) {
        return http.post("api/v1/groups", request, GroupResponse.class);
    }

    public void delete(UUID id) {
        http.delete("api/v1/groups/" + id);
    }
}
