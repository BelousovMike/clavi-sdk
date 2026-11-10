package io.clavi.admin.model;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.time.OffsetDateTime;
import java.util.List;
import java.util.UUID;

@JsonIgnoreProperties(ignoreUnknown = true)
public record ClientResponse(
        UUID id,
        UUID tenantId,
        String clientId,
        String name,
        List<String> redirectUris,
        boolean requirePkce,
        boolean isEnabled,
        OffsetDateTime createdAt
) {
}
