package io.clavi.admin.model;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.time.OffsetDateTime;
import java.util.UUID;

@JsonIgnoreProperties(ignoreUnknown = true)
public record RoleResponse(
        UUID id,
        UUID tenantId,
        String name,
        String description,
        boolean isSystem,
        OffsetDateTime createdAt
) {
}
