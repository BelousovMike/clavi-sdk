using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;
using Refit;

namespace Clavi.Sdk.Admin.Apis;

internal interface IMeApi
{
    [Get("/api/v1/me")]
    Task<MeProfileResponse> GetAsync(CancellationToken cancellationToken = default);

    [Get("/api/v1/me/sessions")]
    Task<PagedResult<MeSessionResponse>> ListSessionsAsync(
        [AliasAs("page")] int page = 1,
        [AliasAs("page_size")] int pageSize = 20,
        CancellationToken cancellationToken = default);

    [Delete("/api/v1/me/sessions/{sessionId}")]
    Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
