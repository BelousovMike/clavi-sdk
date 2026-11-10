using Clavi.Sdk.Admin.Apis;
using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;

namespace Clavi.Sdk.Admin.Resources;

/// <summary>Tenant users (<c>/api/v1/users</c>).</summary>
public sealed class UsersResource
{
    private readonly IUsersApi _api;

    internal UsersResource(IUsersApi api) => _api = api;

    /// <summary>Paged list; optional search string.</summary>
    public Task<PagedResult<UserResponse>> FindAsync(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        CancellationToken cancellationToken = default)
        => _api.FindAsync(page, pageSize, search, cancellationToken);

    public Task<UserResponse> FindOneAsync(Guid id, CancellationToken cancellationToken = default)
        => _api.FindOneAsync(id, cancellationToken);

    public Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        => _api.CreateAsync(request, cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync(id, cancellationToken);

    public Task<IReadOnlyList<RoleResponse>> ListRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        => _api.ListRolesAsync(userId, cancellationToken);

    public Task AssignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        => _api.AssignRoleAsync(userId, roleId, cancellationToken);
}
