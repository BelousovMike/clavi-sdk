package io.clavi.admin.model;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.time.OffsetDateTime;
import java.util.UUID;

@JsonIgnoreProperties(ignoreUnknown = true)
public record GroupResponse(
        UUID id,
        String name,
        String description,
        UUID parentId,
        String path,
        int memberCount,
        OffsetDateTime createdAt
) {
}
