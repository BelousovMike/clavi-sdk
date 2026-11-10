namespace Clavi.Sdk.Admin.Models;

public sealed record GroupResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentId,
    string? Path,
    int MemberCount,
    DateTimeOffset CreatedAt);

public sealed record CreateGroupRequest(string Name, string? Description = null, Guid? ParentId = null);
