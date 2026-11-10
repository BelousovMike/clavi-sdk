using Clavi.Sdk.Admin.Models;
using Refit;

namespace Clavi.Sdk.Admin.Apis;

internal interface ISecurityApi
{
    [Get("/api/v1/security/policies/password")]
    Task<PasswordPolicyResponse> GetPasswordPolicyAsync(CancellationToken cancellationToken = default);
}
