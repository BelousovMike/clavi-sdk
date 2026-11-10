package io.clavi.admin.model;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.util.List;

@JsonIgnoreProperties(ignoreUnknown = true)
public record PagedResult<T>(
        List<T> items,
        int total,
        int page,
        int pageSize
) {
}
