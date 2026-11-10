package io.clavi.spring;

import java.util.ArrayList;
import java.util.List;
import org.springframework.boot.context.properties.ConfigurationProperties;

@ConfigurationProperties(prefix = "clavi")
public class ClaviProperties {

    private String authority = "";
    private String clientId = "";
    private String clientSecret;
    private String callbackPath = "/signin-oidc";
    private String signedOutCallbackPath = "/signout-callback-oidc";
    private List<String> scopes = new ArrayList<>(List.of("openid", "profile", "email"));
    private boolean mapRoles = true;
    private boolean acceptAnyServerCertificate;
    private boolean disablePushedAuthorization = true;
    private String baseUrl;
    private String tenantHost;

    public void validateOidc() {
        if (authority == null || authority.isBlank()) {
            throw new IllegalStateException("clavi.authority is required");
        }
        if (clientId == null || clientId.isBlank()) {
            throw new IllegalStateException("clavi.client-id is required");
        }
    }

    public String resolveAdminBaseUrl() {
        String raw = (baseUrl != null && !baseUrl.isBlank()) ? baseUrl : authority;
        if (raw == null || raw.isBlank()) {
            throw new IllegalStateException("clavi.base-url or clavi.authority is required for Admin");
        }
        return raw.endsWith("/") ? raw : raw + "/";
    }

    public String getAuthority() {
        return authority;
    }

    public void setAuthority(String authority) {
        this.authority = authority == null ? "" : authority.replaceAll("/$", "");
    }

    public String getClientId() {
        return clientId;
    }

    public void setClientId(String clientId) {
        this.clientId = clientId;
    }

    public String getClientSecret() {
        return clientSecret;
    }

    public void setClientSecret(String clientSecret) {
        this.clientSecret = clientSecret;
    }

    public String getCallbackPath() {
        return callbackPath;
    }

    public void setCallbackPath(String callbackPath) {
        this.callbackPath = callbackPath;
    }

    public String getSignedOutCallbackPath() {
        return signedOutCallbackPath;
    }

    public void setSignedOutCallbackPath(String signedOutCallbackPath) {
        this.signedOutCallbackPath = signedOutCallbackPath;
    }

    public List<String> getScopes() {
        return scopes;
    }

    public void setScopes(List<String> scopes) {
        this.scopes = scopes;
    }

    public boolean isMapRoles() {
        return mapRoles;
    }

    public void setMapRoles(boolean mapRoles) {
        this.mapRoles = mapRoles;
    }

    public boolean isAcceptAnyServerCertificate() {
        return acceptAnyServerCertificate;
    }

    public void setAcceptAnyServerCertificate(boolean acceptAnyServerCertificate) {
        this.acceptAnyServerCertificate = acceptAnyServerCertificate;
    }

    public boolean isDisablePushedAuthorization() {
        return disablePushedAuthorization;
    }

    public void setDisablePushedAuthorization(boolean disablePushedAuthorization) {
        this.disablePushedAuthorization = disablePushedAuthorization;
    }

    public String getBaseUrl() {
        return baseUrl;
    }

    public void setBaseUrl(String baseUrl) {
        this.baseUrl = baseUrl;
    }

    public String getTenantHost() {
        return tenantHost;
    }

    public void setTenantHost(String tenantHost) {
        this.tenantHost = tenantHost;
    }
}
