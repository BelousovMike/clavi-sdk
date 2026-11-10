using Clavi.Sdk.Admin.Apis;
using Clavi.Sdk.Admin.Models;

namespace Clavi.Sdk.Admin.Resources;

public sealed class SecurityResource
{
    private readonly ISecurityApi _api;

    internal SecurityResource(ISecurityApi api) => _api = api;

    public Task<PasswordPolicyResponse> GetPasswordPolicyAsync(CancellationToken cancellationToken = default)
        => _api.GetPasswordPolicyAsync(cancellationToken);
}
