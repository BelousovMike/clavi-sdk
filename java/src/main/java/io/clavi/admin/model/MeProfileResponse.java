package io.clavi.admin.model;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.time.OffsetDateTime;
import java.util.List;
import java.util.UUID;

@JsonIgnoreProperties(ignoreUnknown = true)
public record MeProfileResponse(
        UUID id,
        UUID tenantId,
        String username,
        String email,
        boolean emailVerified,
        boolean mfaEnabled,
        OffsetDateTime createdAt,
        List<String> roles
) {
}
