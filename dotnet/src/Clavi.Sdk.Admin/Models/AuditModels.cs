namespace Clavi.Sdk.Admin.Models;

public sealed record AuditEntryHttpItem(
    Guid Id,
    string? EventType,
    string? Message,
    Guid? ActorId,
    string? ActorType,
    string? Action,
    string? ResourceType,
    string? ResourceId,
    string? IpAddress,
    Guid? CorrelationId,
    DateTimeOffset CreatedAt);
