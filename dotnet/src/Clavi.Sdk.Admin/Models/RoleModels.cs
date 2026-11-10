namespace Clavi.Sdk.Admin.Models;

public sealed record RoleResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    bool IsSystem,
    DateTimeOffset CreatedAt);

public sealed record CreateRoleRequest(string Name, string? Description = null);
