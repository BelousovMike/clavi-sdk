using Clavi.Sdk.Admin.Models;
using Refit;

namespace Clavi.Sdk.Admin.Apis;

internal interface IRolesApi
{
    [Get("/api/v1/roles")]
    Task<IReadOnlyList<RoleResponse>> FindAsync(CancellationToken cancellationToken = default);
}
