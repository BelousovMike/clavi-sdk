package io.clavi.admin.resource;

import com.fasterxml.jackson.core.type.TypeReference;
import io.clavi.admin.ClaviHttp;
import io.clavi.admin.model.CreateUserRequest;
import io.clavi.admin.model.PagedResult;
import io.clavi.admin.model.RoleResponse;
import io.clavi.admin.model.UserResponse;

import java.util.List;
import java.util.UUID;

/** Tenant users ({@code /api/v1/users}). */
public final class UsersResource {
    private final ClaviHttp http;

    public UsersResource(ClaviHttp http) {
        this.http = http;
    }

    /** Paged list; pass {@code search} to filter, or {@code null}. */
    public PagedResult<UserResponse> find(int page, int pageSize, String search) {
        String path = ClaviHttp.pagedPath("api/v1/users", page, pageSize, search);
        return http.get(path, new TypeReference<PagedResult<UserResponse>>() {
        });
    }

    public PagedResult<UserResponse> find() {
        return find(1, 20, null);
    }

    public UserResponse findOne(UUID id) {
        return http.get("api/v1/users/" + id, UserResponse.class);
    }

    public UserResponse create(CreateUserRequest request) {
        return http.post("api/v1/users", request, UserResponse.class);
    }

    public void delete(UUID id) {
        http.delete("api/v1/users/" + id);
    }

    public List<RoleResponse> listRoles(UUID userId) {
        return http.get("api/v1/users/" + userId + "/roles", new TypeReference<List<RoleResponse>>() {
        });
    }
}
