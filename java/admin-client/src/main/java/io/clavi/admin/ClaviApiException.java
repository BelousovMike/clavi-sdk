package io.clavi.admin;

public final class ClaviApiException extends RuntimeException {
    private final int status;
    private final String code;
    private final String body;

    public ClaviApiException(int status, String code, String body) {
        super(format(status, code, body));
        this.status = status;
        this.code = code;
        this.body = body;
    }

    public int status() {
        return status;
    }

    public String code() {
        return code;
    }

    public String body() {
        return body;
    }

    private static String format(int status, String code, String body) {
        String truncated = body == null ? "" : body.length() <= 200 ? body : body.substring(0, 200);
        return code == null
                ? "Clavi HTTP " + status + ": " + truncated
                : "Clavi HTTP " + status + " [" + code + "]: " + truncated;
    }
}
