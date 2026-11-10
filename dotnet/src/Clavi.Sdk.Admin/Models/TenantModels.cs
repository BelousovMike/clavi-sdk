namespace Clavi.Sdk.Admin.Models;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Slug,
    string? State,
    DateTimeOffset CreatedAt);
