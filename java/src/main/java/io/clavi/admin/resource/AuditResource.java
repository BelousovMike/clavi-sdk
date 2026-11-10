package io.clavi.admin.resource;

import com.fasterxml.jackson.core.type.TypeReference;
import io.clavi.admin.ClaviHttp;
import io.clavi.admin.model.AuditEntryHttpItem;
import io.clavi.admin.model.PagedResult;

public final class AuditResource {
    private final ClaviHttp http;

    public AuditResource(ClaviHttp http) {
        this.http = http;
    }

    public PagedResult<AuditEntryHttpItem> find(int page, int pageSize) {
        return http.get(
                ClaviHttp.pagedPath("api/v1/audit/entries", page, pageSize, null),
                new TypeReference<PagedResult<AuditEntryHttpItem>>() {
                });
    }
}
