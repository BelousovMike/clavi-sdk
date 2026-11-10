package io.clavi.admin.model;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.time.OffsetDateTime;
import java.util.UUID;

@JsonIgnoreProperties(ignoreUnknown = true)
public record MeSessionResponse(
        UUID id,
        UUID userId,
        OffsetDateTime createdAt,
        OffsetDateTime lastActivityAt,
        OffsetDateTime expiresAt,
        boolean isCurrent
) {
}
