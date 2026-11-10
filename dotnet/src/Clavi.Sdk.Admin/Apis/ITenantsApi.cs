using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;
using Refit;

namespace Clavi.Sdk.Admin.Apis;

internal interface ITenantsApi
{
    [Get("/api/v1/system/tenants")]
    Task<PagedResult<TenantResponse>> FindAsync(
        [AliasAs("page")] int page = 1,
        [AliasAs("page_size")] int pageSize = 20,
        CancellationToken cancellationToken = default);

    [Get("/api/v1/system/tenants/{id}")]
    Task<TenantResponse> FindOneAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/api/v1/system/tenants/{id}/config")]
    Task<TenantConfigResponse> GetConfigAsync(Guid id, CancellationToken cancellationToken = default);

    [Put("/api/v1/system/tenants/{id}/config")]
    Task<TenantConfigResponse> PutConfigAsync(Guid id, [Body] TenantConfigBody body, CancellationToken cancellationToken = default);
}
