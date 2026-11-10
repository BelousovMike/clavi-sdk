using Clavi.Sdk.Admin.Apis;
using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;

namespace Clavi.Sdk.Admin.Resources;

public sealed class MeResource
{
    private readonly IMeApi _api;

    internal MeResource(IMeApi api) => _api = api;

    public Task<MeProfileResponse> GetAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync(cancellationToken);

    public Task<PagedResult<MeSessionResponse>> ListSessionsAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
        => _api.ListSessionsAsync(page, pageSize, cancellationToken);

    public Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
        => _api.RevokeSessionAsync(sessionId, cancellationToken);
}
