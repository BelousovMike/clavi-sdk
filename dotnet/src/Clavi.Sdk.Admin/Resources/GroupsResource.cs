using Clavi.Sdk.Admin.Apis;
using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;

namespace Clavi.Sdk.Admin.Resources;

public sealed class GroupsResource
{
    private readonly IGroupsApi _api;

    internal GroupsResource(IGroupsApi api) => _api = api;

    public Task<PagedResult<GroupResponse>> FindAsync(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        CancellationToken cancellationToken = default)
        => _api.FindAsync(page, pageSize, search, cancellationToken);

    public Task<GroupResponse> FindOneAsync(Guid id, CancellationToken cancellationToken = default)
        => _api.FindOneAsync(id, cancellationToken);

    public Task<GroupResponse> CreateAsync(CreateGroupRequest request, CancellationToken cancellationToken = default)
        => _api.CreateAsync(request, cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync(id, cancellationToken);
}
