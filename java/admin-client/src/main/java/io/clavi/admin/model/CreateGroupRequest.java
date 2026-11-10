package io.clavi.admin.model;

import java.util.UUID;

public record CreateGroupRequest(String name, String description, UUID parentId) {
    public CreateGroupRequest(String name) {
        this(name, null, null);
    }
}
