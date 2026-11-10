using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;
using Refit;

namespace Clavi.Sdk.Admin.Apis;

internal interface IClientsApi
{
    [Get("/api/v1/oidc/clients")]
    Task<PagedResult<ClientResponse>> FindAsync(
        [AliasAs("page")] int page = 1,
        [AliasAs("page_size")] int pageSize = 20,
        [AliasAs("search")] string? search = null,
        CancellationToken cancellationToken = default);

    [Get("/api/v1/oidc/clients/{id}")]
    Task<ClientResponse> FindOneAsync(Guid id, CancellationToken cancellationToken = default);

    [Post("/api/v1/oidc/clients")]
    Task<ClientResponse> CreateAsync([Body] CreateClientRequest request, CancellationToken cancellationToken = default);
}
