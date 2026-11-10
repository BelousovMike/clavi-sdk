package io.clavi.admin;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;

import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;
import java.io.IOException;
import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.security.SecureRandom;
import java.security.cert.X509Certificate;
import java.time.Duration;
import java.util.Objects;

public final class ClaviHttp implements AutoCloseable {
    private final ClaviClientConfig config;
    private final HttpClient http;
    private final ObjectMapper mapper;
    private final SessionCookieHandler cookies = new SessionCookieHandler();

    static {
        String key = "jdk.httpclient.allowRestrictedHeaders";
        String existing = System.getProperty(key);
        if (existing == null || existing.isBlank()) {
            System.setProperty(key, "host");
        } else if (!existing.toLowerCase().contains("host")) {
            System.setProperty(key, existing + ",host");
        }
    }

    public ClaviHttp(ClaviClientConfig config) {
        this.config = Objects.requireNonNull(config);
        this.mapper = new ObjectMapper()
                .registerModule(new JavaTimeModule())
                .disable(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS)
                .disable(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES);

        HttpClient.Builder builder = HttpClient.newBuilder()
                .connectTimeout(Duration.ofSeconds(30))
                .cookieHandler(cookies)
                .followRedirects(HttpClient.Redirect.NORMAL);

        if (config.acceptAnyServerCertificate()) {
            builder.sslContext(trustAllSsl());
        }

        this.http = builder.build();
    }

    public ClaviClientConfig config() {
        return config;
    }

    public ObjectMapper mapper() {
        return mapper;
    }

    public <T> T get(String relativePath, Class<T> type) {
        return send("GET", relativePath, null, type, null);
    }

    public <T> T get(String relativePath, TypeReference<T> type) {
        return send("GET", relativePath, null, null, type);
    }

    public <T> T post(String relativePath, Object body, Class<T> type) {
        return send("POST", relativePath, body, type, null);
    }

    public void post(String relativePath, Object body) {
        send("POST", relativePath, body, Void.class, null);
    }

    public void delete(String relativePath) {
        send("DELETE", relativePath, null, Void.class, null);
    }

    public static String pagedPath(String path, int page, int pageSize, String search) {
        StringBuilder sb = new StringBuilder(path.contains("?") ? path + "&" : path + "?");
        sb.append("page=").append(page).append("&page_size=").append(pageSize);
        if (search != null && !search.isBlank()) {
            sb.append("&search=").append(URLEncoder.encode(search.trim(), StandardCharsets.UTF_8));
        }
        return sb.toString();
    }

    private <T> T send(
            String method,
            String relativePath,
            Object body,
            Class<T> type,
            TypeReference<T> typeRef) {
        try {
            String path = relativePath.startsWith("/") ? relativePath.substring(1) : relativePath;
            URI uri = config.baseUrl().resolve(path);
            HttpRequest.Builder req = HttpRequest.newBuilder(uri)
                    .timeout(Duration.ofSeconds(60))
                    .header("Origin", config.origin())
                    .header("Accept", "application/json");
            if (!config.tenantHost().equalsIgnoreCase(uri.getHost())) {
                req.header("Host", config.tenantHost());
            }

            if (body != null) {
                byte[] json = mapper.writeValueAsBytes(body);
                req.header("Content-Type", "application/json");
                req.method(method, HttpRequest.BodyPublishers.ofByteArray(json));
            } else {
                req.method(method, HttpRequest.BodyPublishers.noBody());
            }

            HttpResponse<String> response = http.send(req.build(), HttpResponse.BodyHandlers.ofString());
            if (response.statusCode() >= 400) {
                throw toException(response);
            }
            if ((type == Void.class && typeRef == null)
                    || response.statusCode() == 204
                    || response.body() == null
                    || response.body().isBlank()) {
                return null;
            }
            if (typeRef != null) {
                return mapper.readValue(response.body(), typeRef);
            }
            return mapper.readValue(response.body(), type);
        } catch (ClaviApiException e) {
            throw e;
        } catch (IOException | InterruptedException e) {
            if (e instanceof InterruptedException) {
                Thread.currentThread().interrupt();
            }
            throw new ClaviApiException(0, null, e.getMessage());
        }
    }

    private ClaviApiException toException(HttpResponse<String> response) {
        String body = response.body();
        String code = null;
        try {
            JsonNode node = mapper.readTree(body);
            if (node.hasNonNull("code")) {
                code = node.get("code").asText();
            } else if (node.has("extensions") && node.get("extensions").hasNonNull("code")) {
                code = node.get("extensions").get("code").asText();
            }
        } catch (Exception ignored) {
            // keep body only
        }
        return new ClaviApiException(response.statusCode(), code, body);
    }

    private static SSLContext trustAllSsl() {
        try {
            TrustManager[] trustAll = new TrustManager[]{
                    new X509TrustManager() {
                        public void checkClientTrusted(X509Certificate[] chain, String authType) {
                        }

                        public void checkServerTrusted(X509Certificate[] chain, String authType) {
                        }

                        public X509Certificate[] getAcceptedIssuers() {
                            return new X509Certificate[0];
                        }
                    }
            };
            SSLContext ctx = SSLContext.getInstance("TLS");
            ctx.init(null, trustAll, new SecureRandom());
            return ctx;
        } catch (Exception e) {
            throw new IllegalStateException("Unable to create trust-all SSLContext", e);
        }
    }

    @Override
    public void close() {
        cookies.clear();
    }
}
