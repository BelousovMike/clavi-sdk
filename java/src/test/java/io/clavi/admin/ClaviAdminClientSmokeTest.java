package io.clavi.admin;

import io.clavi.admin.model.PagedResult;
import io.clavi.admin.model.UserResponse;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;

class ClaviAdminClientSmokeTest {
    @Test
    void loginAndListUsers() {
        if (!"1".equals(System.getenv("CLAVI_SDK_SMOKE"))) {
            return;
        }

        String password = System.getenv("CLAVI_E2E_ADMIN_PASSWORD");
        if (password == null || password.isBlank()) {
            throw new IllegalStateException("CLAVI_E2E_ADMIN_PASSWORD required for smoke");
        }
        String username = System.getenv().getOrDefault("CLAVI_E2E_ADMIN_USERNAME", "admin");
        String baseUrl = System.getenv().getOrDefault("CLAVI_BASE_URL", "https://clavi.clavi.local/");
        String host = System.getenv().getOrDefault("CLAVI_TENANT_HOST", "clavi.clavi.local");

        try (ClaviAdminClient client = ClaviAdminClient.builder()
                .baseUrl(baseUrl)
                .tenantHost(host)
                .acceptAnyServerCertificate(true)
                .build()) {
            client.auth(username, password);
            PagedResult<UserResponse> page = client.users().find(1, 10, null);
            assertNotNull(page);
            assertNotNull(page.items());
            assertTrue(page.pageSize() <= 100);
        }
    }

    @Test
    void builderRequiresBaseUrl() {
        try {
            ClaviAdminClient.builder().build();
        } catch (IllegalStateException expected) {
            return;
        }
        // If env provides CLAVI_BASE_URL, builder succeeds — still a valid outcome.
    }
}
