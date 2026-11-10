namespace Clavi.Sdk.Admin.Models;

public sealed record UserResponse(Guid Id, Guid TenantId, string Username, DateTimeOffset CreatedAt);

public sealed record CreateUserRequest(string Username, string Password, string? Email = null);
