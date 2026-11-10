namespace Clavi.Sdk.AspNetCore;

/// <summary>
/// OIDC relying-party options for an ASP.NET Core app that signs users in via Clavi.
/// Bind from the <c>Clavi</c> (or <c>Clavi:Oidc</c>) config section.
/// </summary>
public sealed class ClaviOidcOptions
{
    public const string SectionName = "Clavi";

    /// <summary>Clavi issuer / authority, e.g. https://clavi.example</summary>
    public string Authority { get; set; } = "";

    /// <summary>OIDC client_id registered in Clavi for this app.</summary>
    public string ClientId { get; set; } = "";

    /// <summary>Optional client secret (confidential clients). Public+PKCE leave empty.</summary>
    public string? ClientSecret { get; set; }

    public string CallbackPath { get; set; } = "/signin-oidc";

    public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";

    /// <summary>Scopes requested at authorize. Defaults to openid profile email.</summary>
    public string[] Scopes { get; set; } = ["openid", "profile", "email"];

    /// <summary>Map Clavi <c>roles</c> claim into <see cref="System.Security.Claims.ClaimTypes.Role"/>.</summary>
    public bool MapRoles { get; set; } = true;

    /// <summary>Disable OAuth PAR (needed when public URL/port differs from container Host).</summary>
    public bool DisablePushedAuthorization { get; set; } = true;

    /// <summary>Accept self-signed TLS when talking to Clavi (lab only).</summary>
    public bool AcceptAnyServerCertificate { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Authority))
            throw new InvalidOperationException("Clavi:Authority is required.");
        if (string.IsNullOrWhiteSpace(ClientId))
            throw new InvalidOperationException("Clavi:ClientId is required.");
    }
}
