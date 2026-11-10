package io.clavi.admin.model;

public record CreateUserRequest(String username, String password, String email) {
    public CreateUserRequest(String username, String password) {
        this(username, password, null);
    }
}
