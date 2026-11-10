using Clavi.Sdk.Admin.Apis;
using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;

namespace Clavi.Sdk.Admin.Resources;

public sealed class TenantsResource
{
    private readonly ITenantsApi _api;

    internal TenantsResource(ITenantsApi api) => _api = api;

    public Task<PagedResult<TenantResponse>> FindAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
        => _api.FindAsync(page, pageSize, cancellationToken);

    public Task<TenantResponse> FindOneAsync(Guid id, CancellationToken cancellationToken = default)
        => _api.FindOneAsync(id, cancellationToken);

    public Task<TenantConfigResponse> GetConfigAsync(Guid id, CancellationToken cancellationToken = default)
        => _api.GetConfigAsync(id, cancellationToken);

    public Task<TenantConfigResponse> PutConfigAsync(
        Guid id,
        TenantConfigBody body,
        CancellationToken cancellationToken = default)
        => _api.PutConfigAsync(id, body, cancellationToken);
}
