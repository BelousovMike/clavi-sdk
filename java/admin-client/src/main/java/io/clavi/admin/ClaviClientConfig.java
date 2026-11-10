package io.clavi.admin;

import java.net.URI;
import java.util.Objects;

/** Connection settings: base URL, optional tenant Host override, Origin for CSRF. */
public final class ClaviClientConfig {
    private final URI baseUrl;
    private final String tenantHost;
    private final String origin;
    private final boolean acceptAnyServerCertificate;

    private ClaviClientConfig(Builder b) {
        this.baseUrl = Objects.requireNonNull(b.baseUrl, "baseUrl");
        this.tenantHost = b.tenantHost != null && !b.tenantHost.isBlank()
                ? b.tenantHost
                : b.baseUrl.getHost();
        this.origin = b.origin != null && !b.origin.isBlank()
                ? b.origin
                : b.baseUrl.getScheme() + "://" + b.baseUrl.getAuthority();
        this.acceptAnyServerCertificate = b.acceptAnyServerCertificate;
    }

    public URI baseUrl() {
        return baseUrl;
    }

    public String tenantHost() {
        return tenantHost;
    }

    public String origin() {
        return origin;
    }

    public boolean acceptAnyServerCertificate() {
        return acceptAnyServerCertificate;
    }

    public static Builder builder() {
        return new Builder();
    }

    public static final class Builder {
        private URI baseUrl;
        private String tenantHost;
        private String origin;
        private boolean acceptAnyServerCertificate;

        public Builder baseUrl(String baseUrl) {
            String normalized = baseUrl.endsWith("/") ? baseUrl : baseUrl + "/";
            this.baseUrl = URI.create(normalized);
            return this;
        }

        public Builder baseUrl(URI baseUrl) {
            String s = baseUrl.toString();
            this.baseUrl = URI.create(s.endsWith("/") ? s : s + "/");
            return this;
        }

        public Builder tenantHost(String tenantHost) {
            this.tenantHost = tenantHost;
            return this;
        }

        public Builder origin(String origin) {
            this.origin = origin;
            return this;
        }

        /** Only for local stacks with a self-signed certificate. */
        public Builder acceptAnyServerCertificate(boolean value) {
            this.acceptAnyServerCertificate = value;
            return this;
        }

        /** Fills blanks from CLAVI_BASE_URL / CLAVI_TENANT_HOST / CLAVI_ORIGIN. */
        public Builder fromEnv() {
            String envBase = System.getenv("CLAVI_BASE_URL");
            if (baseUrl == null && envBase != null && !envBase.isBlank()) {
                baseUrl(envBase);
            }
            if (tenantHost == null) {
                tenantHost = System.getenv("CLAVI_TENANT_HOST");
            }
            if (origin == null) {
                origin = System.getenv("CLAVI_ORIGIN");
            }
            return this;
        }

        public ClaviClientConfig build() {
            if (baseUrl == null) {
                fromEnv();
            }
            if (baseUrl == null) {
                throw new IllegalStateException("baseUrl / CLAVI_BASE_URL is required");
            }
            return new ClaviClientConfig(this);
        }
    }
}
