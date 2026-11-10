using Clavi.Sdk.Admin.Apis;
using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;

namespace Clavi.Sdk.Admin.Resources;

public sealed class ClientsResource
{
    private readonly IClientsApi _api;

    internal ClientsResource(IClientsApi api) => _api = api;

    public Task<PagedResult<ClientResponse>> FindAsync(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        CancellationToken cancellationToken = default)
        => _api.FindAsync(page, pageSize, search, cancellationToken);

    public Task<ClientResponse> FindOneAsync(Guid id, CancellationToken cancellationToken = default)
        => _api.FindOneAsync(id, cancellationToken);

    public Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
        => _api.CreateAsync(request, cancellationToken);
}
