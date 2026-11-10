package io.clavi.admin;

import java.io.IOException;
import java.net.CookieHandler;
import java.net.URI;
import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

final class SessionCookieHandler extends CookieHandler {
    private final Map<String, String> cookies = new LinkedHashMap<>();

    @Override
    public Map<String, List<String>> get(URI uri, Map<String, List<String>> requestHeaders) {
        if (cookies.isEmpty()) {
            return Collections.emptyMap();
        }
        StringBuilder value = new StringBuilder();
        cookies.forEach((k, v) -> {
            if (!value.isEmpty()) {
                value.append("; ");
            }
            value.append(k).append('=').append(v);
        });
        return Map.of("Cookie", List.of(value.toString()));
    }

    @Override
    public void put(URI uri, Map<String, List<String>> responseHeaders) throws IOException {
        List<String> setCookies = new ArrayList<>();
        responseHeaders.forEach((name, values) -> {
            if (name != null && name.equalsIgnoreCase("Set-Cookie") && values != null) {
                setCookies.addAll(values);
            }
        });
        for (String line : setCookies) {
            String pair = line.split(";", 2)[0];
            int eq = pair.indexOf('=');
            if (eq > 0) {
                cookies.put(pair.substring(0, eq).trim(), pair.substring(eq + 1).trim());
            }
        }
    }

    void clear() {
        cookies.clear();
    }
}
