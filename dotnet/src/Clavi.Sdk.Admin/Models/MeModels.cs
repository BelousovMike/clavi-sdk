namespace Clavi.Sdk.Admin.Models;

public sealed record MeProfileResponse(
    Guid Id,
    Guid TenantId,
    string Username,
    string? Email,
    bool EmailVerified,
    bool MfaEnabled,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string>? Roles);

public sealed record MeSessionResponse(
    Guid Id,
    Guid UserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastActivityAt,
    DateTimeOffset? ExpiresAt,
    bool IsCurrent);
