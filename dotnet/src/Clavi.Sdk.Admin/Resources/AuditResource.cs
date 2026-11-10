using Clavi.Sdk.Admin.Apis;
using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;

namespace Clavi.Sdk.Admin.Resources;

public sealed class AuditResource
{
    private readonly IAuditApi _api;

    internal AuditResource(IAuditApi api) => _api = api;

    public Task<PagedResult<AuditEntryHttpItem>> FindAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
        => _api.FindAsync(page, pageSize, cancellationToken);
}
