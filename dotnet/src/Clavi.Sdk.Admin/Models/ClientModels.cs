namespace Clavi.Sdk.Admin.Models;

public sealed record ClientResponse(
    Guid Id,
    Guid TenantId,
    string ClientId,
    string? Name,
    IReadOnlyList<string>? RedirectUris,
    bool RequirePkce,
    bool IsEnabled,
    DateTimeOffset CreatedAt);

public sealed record CreateClientRequest(
    string ClientId,
    string Name,
    IReadOnlyList<string> RedirectUris,
    IReadOnlyList<string>? PostLogoutRedirectUris = null,
    bool RequirePkce = true,
    bool RequireConsent = false);
