package io.clavi.admin.resource;

import com.fasterxml.jackson.core.type.TypeReference;
import io.clavi.admin.ClaviHttp;
import io.clavi.admin.model.MeProfileResponse;
import io.clavi.admin.model.MeSessionResponse;
import io.clavi.admin.model.PagedResult;

import java.util.UUID;

public final class MeResource {
    private final ClaviHttp http;

    public MeResource(ClaviHttp http) {
        this.http = http;
    }

    public MeProfileResponse get() {
        return http.get("api/v1/me", MeProfileResponse.class);
    }

    public PagedResult<MeSessionResponse> listSessions(int page, int pageSize) {
        return http.get(
                ClaviHttp.pagedPath("api/v1/me/sessions", page, pageSize, null),
                new TypeReference<PagedResult<MeSessionResponse>>() {
                });
    }

    public void revokeSession(UUID sessionId) {
        http.delete("api/v1/me/sessions/" + sessionId);
    }
}
