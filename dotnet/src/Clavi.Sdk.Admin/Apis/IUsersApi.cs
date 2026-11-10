using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;
using Refit;

namespace Clavi.Sdk.Admin.Apis;

internal interface IUsersApi
{
    [Get("/api/v1/users")]
    Task<PagedResult<UserResponse>> FindAsync(
        [AliasAs("page")] int page = 1,
        [AliasAs("page_size")] int pageSize = 20,
        [AliasAs("search")] string? search = null,
        CancellationToken cancellationToken = default);

    [Get("/api/v1/users/{id}")]
    Task<UserResponse> FindOneAsync(Guid id, CancellationToken cancellationToken = default);

    [Post("/api/v1/users")]
    Task<UserResponse> CreateAsync([Body] CreateUserRequest request, CancellationToken cancellationToken = default);

    [Delete("/api/v1/users/{id}")]
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/api/v1/users/{userId}/roles")]
    Task<IReadOnlyList<RoleResponse>> ListRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    [Post("/api/v1/users/{userId}/roles/{roleId}")]
    Task AssignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    [Delete("/api/v1/users/{userId}/roles/{roleId}")]
    Task UnassignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}
