using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Clavi.Sdk.AspNetCore;

/// <summary>Maps Clavi <c>roles</c> arrays from userinfo / access token into role claims.</summary>
public static class ClaviOidcRoleClaims
{
    /// <summary>Hooks role mapping onto OIDC events without dropping existing handlers.</summary>
    public static void Attach(OpenIdConnectOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var events = options.Events ??= new OpenIdConnectEvents();
        events.OnUserInformationReceived = Chain(events.OnUserInformationReceived, MapFromUserInfo);
        events.OnTokenValidated = Chain(events.OnTokenValidated, MapFromAccessToken);
    }

    public static void MapFromUserInfo(UserInformationReceivedContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.User.RootElement.TryGetProperty("roles", out var roles)
            && roles.ValueKind == JsonValueKind.Array)
        {
            AddRoleClaims(context.Principal, roles);
        }
    }

    public static void MapFromAccessToken(TokenValidatedContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var access = context.TokenEndpointResponse?.AccessToken;
        if (string.IsNullOrEmpty(access))
            return;

        var parts = access.Split('.');
        if (parts.Length < 2)
            return;

        try
        {
            var json = Base64UrlDecode(parts[1]);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("roles", out var roles)
                && roles.ValueKind == JsonValueKind.Array)
            {
                AddRoleClaims(context.Principal, roles);
            }
        }
        catch (FormatException)
        {
            // Malformed JWT payload — skip role mapping.
        }
        catch (JsonException)
        {
            // Non-JSON payload — skip role mapping.
        }
    }

    public static void AddRoleClaims(ClaimsPrincipal? principal, JsonElement roles)
    {
        if (principal?.Identity is not ClaimsIdentity id)
            return;

        foreach (var role in roles.EnumerateArray())
        {
            var name = role.GetString();
            if (string.IsNullOrWhiteSpace(name))
                continue;
            if (!id.HasClaim(ClaimTypes.Role, name))
                id.AddClaim(new Claim(ClaimTypes.Role, name));
        }
    }

    private static Func<T, Task> Chain<T>(Func<T, Task>? prior, Action<T> next) => async ctx =>
    {
        next(ctx);
        if (prior is not null)
            await prior(ctx).ConfigureAwait(false);
    };

    private static string Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }
}
