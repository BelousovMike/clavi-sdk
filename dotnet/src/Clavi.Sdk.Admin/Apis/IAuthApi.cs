using Clavi.Sdk.Admin.Models;
using Refit;

namespace Clavi.Sdk.Admin.Apis;

internal interface IAuthApi
{
    [Post("/api/v1/auth/login")]
    Task LoginAsync([Body] LoginRequest request, CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/logout")]
    Task LogoutAsync(CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/logout-all")]
    Task LogoutAllAsync(CancellationToken cancellationToken = default);
}
