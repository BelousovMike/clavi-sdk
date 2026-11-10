using Clavi.Sdk.Admin.Apis;
using Clavi.Sdk.Admin.Models;

namespace Clavi.Sdk.Admin.Resources;

public sealed class RolesResource
{
    private readonly IRolesApi _api;

    internal RolesResource(IRolesApi api) => _api = api;

    public Task<IReadOnlyList<RoleResponse>> FindAsync(CancellationToken cancellationToken = default)
        => _api.FindAsync(cancellationToken);
}
