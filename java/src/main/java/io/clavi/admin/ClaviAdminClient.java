package io.clavi.admin;

import io.clavi.admin.model.LoginRequest;
import io.clavi.admin.resource.AuditResource;
import io.clavi.admin.resource.ClientsResource;
import io.clavi.admin.resource.GroupsResource;
import io.clavi.admin.resource.MeResource;
import io.clavi.admin.resource.RolesResource;
import io.clavi.admin.resource.SecurityResource;
import io.clavi.admin.resource.TenantsResource;
import io.clavi.admin.resource.UsersResource;

/**
 * Admin API entry point. Build a client, call {@link #auth}, then use
 * {@link #users()}, {@link #groups()}, and the other resources. The session
 * cookie stays on this instance.
 *
 * <pre>{@code
 * try (var client = ClaviAdminClient.builder()
 *         .baseUrl("https://clavi.example/")
 *         .build()) {
 *     client.auth("admin", password);
 *     var page = client.users().find(1, 20, null);
 * }
 * }</pre>
 */
public final class ClaviAdminClient implements AutoCloseable {
    private final ClaviHttp http;
    private final UsersResource users;
    private final GroupsResource groups;
    private final RolesResource roles;
    private final ClientsResource clients;
    private final AuditResource audit;
    private final TenantsResource tenants;
    private final SecurityResource security;
    private final MeResource me;

    public ClaviAdminClient(ClaviClientConfig config) {
        this.http = new ClaviHttp(config);
        this.users = new UsersResource(http);
        this.groups = new GroupsResource(http);
        this.roles = new RolesResource(http);
        this.clients = new ClientsResource(http);
        this.audit = new AuditResource(http);
        this.tenants = new TenantsResource(http);
        this.security = new SecurityResource(http);
        this.me = new MeResource(http);
    }

    public static Builder builder() {
        return new Builder();
    }

    /** BFF login; keeps the session cookie on this client. */
    public void auth(String username, String password) {
        http.post("api/v1/auth/login", new LoginRequest(username, password));
    }

    public void logout() {
        http.post("api/v1/auth/logout", null);
    }

    public UsersResource users() {
        return users;
    }

    public GroupsResource groups() {
        return groups;
    }

    public RolesResource roles() {
        return roles;
    }

    public ClientsResource clients() {
        return clients;
    }

    public AuditResource audit() {
        return audit;
    }

    public TenantsResource tenants() {
        return tenants;
    }

    public SecurityResource security() {
        return security;
    }

    public MeResource me() {
        return me;
    }

    /** Low-level HTTP helper if you need a path without a typed resource. */
    public ClaviHttp http() {
        return http;
    }

    @Override
    public void close() {
        http.close();
    }

    public static final class Builder {
        private final ClaviClientConfig.Builder config = ClaviClientConfig.builder();

        public Builder baseUrl(String baseUrl) {
            config.baseUrl(baseUrl);
            return this;
        }

        public Builder tenantHost(String tenantHost) {
            config.tenantHost(tenantHost);
            return this;
        }

        public Builder origin(String origin) {
            config.origin(origin);
            return this;
        }

        public Builder acceptAnyServerCertificate(boolean value) {
            config.acceptAnyServerCertificate(value);
            return this;
        }

        /** Fills blanks from CLAVI_BASE_URL / CLAVI_TENANT_HOST / CLAVI_ORIGIN. */
        public Builder fromEnv() {
            config.fromEnv();
            return this;
        }

        public ClaviAdminClient build() {
            return new ClaviAdminClient(config.build());
        }
    }
}
