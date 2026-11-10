package io.clavi.admin.model;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.time.OffsetDateTime;
import java.util.UUID;

@JsonIgnoreProperties(ignoreUnknown = true)
public record TenantResponse(
        UUID id,
        String name,
        String slug,
        String state,
        OffsetDateTime createdAt
) {
}
