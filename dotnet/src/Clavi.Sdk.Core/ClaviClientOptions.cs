namespace Clavi.Sdk.Core;

/// <summary>
/// How to reach Clavi. Bind from the <c>Clavi</c> config section or set in code.
/// <see cref="BaseUrl"/> is required; <see cref="TenantHost"/> defaults to the URL host.
/// </summary>
public sealed class ClaviClientOptions
{
    public const string SectionName = "Clavi";

    /// <summary>Public base URL, e.g. https://clavi.example/</summary>
    public Uri BaseUrl { get; set; } = default!;

    /// <summary>Value for the Host header when it must differ from BaseUrl's host.</summary>
    public string? TenantHost { get; set; }

    /// <summary>Origin sent on mutating calls (CSRF). Defaults to the BaseUrl origin.</summary>
    public string? Origin { get; set; }

    /// <summary>Turn on only for local stacks with a self-signed cert.</summary>
    public bool AcceptAnyServerCertificate { get; set; }

    public void Validate()
    {
        if (BaseUrl is null)
            throw new InvalidOperationException("Clavi:BaseUrl is required.");
    }

    public string ResolveOrigin()
        => string.IsNullOrWhiteSpace(Origin) ? BaseUrl.GetLeftPart(UriPartial.Authority) : Origin;

    public string ResolveTenantHost()
        => string.IsNullOrWhiteSpace(TenantHost) ? BaseUrl.Host : TenantHost;
}
