package io.clavi.admin.model;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.time.OffsetDateTime;
import java.util.UUID;

@JsonIgnoreProperties(ignoreUnknown = true)
public record AuditEntryHttpItem(
        UUID id,
        String eventType,
        String message,
        UUID actorId,
        String actorType,
        String action,
        String resourceType,
        String resourceId,
        String ipAddress,
        UUID correlationId,
        OffsetDateTime createdAt
) {
}
