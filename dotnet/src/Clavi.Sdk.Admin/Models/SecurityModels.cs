namespace Clavi.Sdk.Admin.Models;

public sealed record PasswordPolicyResponse(
    int MinLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireDigit,
    bool RequireSpecial,
    int HistorySize,
    int MaxAgeDays,
    bool BreachCheckEnabled);
