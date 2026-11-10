using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Core;
using Refit;

namespace Clavi.Sdk.Admin.Apis;

internal interface IAuditApi
{
    [Get("/api/v1/audit/entries")]
    Task<PagedResult<AuditEntryHttpItem>> FindAsync(
        [AliasAs("page")] int page = 1,
        [AliasAs("page_size")] int pageSize = 20,
        CancellationToken cancellationToken = default);
}
