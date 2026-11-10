package io.clavi.admin.model;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

@JsonIgnoreProperties(ignoreUnknown = true)
public record PasswordPolicyResponse(
        int minLength,
        boolean requireUppercase,
        boolean requireLowercase,
        boolean requireDigit,
        boolean requireSpecial,
        int historySize,
        int maxAgeDays,
        boolean breachCheckEnabled
) {
}
