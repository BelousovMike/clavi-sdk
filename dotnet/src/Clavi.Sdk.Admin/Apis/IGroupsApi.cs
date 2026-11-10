using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;
using Refit;

namespace Clavi.Sdk.Admin.Apis;

internal interface IGroupsApi
{
    [Get("/api/v1/groups")]
    Task<PagedResult<GroupResponse>> FindAsync(
        [AliasAs("page")] int page = 1,
        [AliasAs("page_size")] int pageSize = 20,
        [AliasAs("search")] string? search = null,
        CancellationToken cancellationToken = default);

    [Get("/api/v1/groups/{id}")]
    Task<GroupResponse> FindOneAsync(Guid id, CancellationToken cancellationToken = default);

    [Post("/api/v1/groups")]
    Task<GroupResponse> CreateAsync([Body] CreateGroupRequest request, CancellationToken cancellationToken = default);

    [Delete("/api/v1/groups/{id}")]
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
