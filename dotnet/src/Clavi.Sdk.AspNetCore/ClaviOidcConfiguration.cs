using Microsoft.Extensions.Configuration;

namespace Clavi.Sdk.AspNetCore;

/// <summary>
/// Fluent Clavi OIDC setup (Serilog-style): chain <see cref="ReadFrom"/>, then overrides.
/// </summary>
public sealed class ClaviOidcConfiguration
{
    private readonly List<Action<ClaviOidcOptions>> _actions = [];

    public ClaviOidcConfiguration()
        => ReadFrom = new ClaviOidcReadFromConfigurationSettings(this);

    /// <summary>Load settings from configuration (appsettings, env, etc.).</summary>
    public ClaviOidcReadFromConfigurationSettings ReadFrom { get; }

    public ClaviOidcConfiguration Authority(string authority)
        => Set(o => o.Authority = authority);

    public ClaviOidcConfiguration ClientId(string clientId)
        => Set(o => o.ClientId = clientId);

    public ClaviOidcConfiguration ClientSecret(string? clientSecret)
        => Set(o => o.ClientSecret = clientSecret);

    public ClaviOidcConfiguration CallbackPath(string path)
        => Set(o => o.CallbackPath = path);

    public ClaviOidcConfiguration SignedOutCallbackPath(string path)
        => Set(o => o.SignedOutCallbackPath = path);

    public ClaviOidcConfiguration Scopes(params string[] scopes)
        => Set(o => o.Scopes = scopes);

    public ClaviOidcConfiguration MapRoles(bool enabled = true)
        => Set(o => o.MapRoles = enabled);

    public ClaviOidcConfiguration DisablePushedAuthorization(bool disable = true)
        => Set(o => o.DisablePushedAuthorization = disable);

    public ClaviOidcConfiguration AcceptAnyServerCertificate(bool accept = true)
        => Set(o => o.AcceptAnyServerCertificate = accept);

    /// <summary>Imperative tweak when a named fluent method is missing.</summary>
    public ClaviOidcConfiguration Configure(Action<ClaviOidcOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        return Set(configure);
    }

    internal void ApplyTo(ClaviOidcOptions options)
    {
        foreach (var action in _actions)
            action(options);
    }

    internal ClaviOidcConfiguration Set(Action<ClaviOidcOptions> action)
    {
        _actions.Add(action);
        return this;
    }
}

/// <summary>Serilog-like <c>ReadFrom.Configuration(...)</c> entry.</summary>
public sealed class ClaviOidcReadFromConfigurationSettings(ClaviOidcConfiguration parent)
{
    /// <summary>
    /// Binds the <paramref name="sectionName"/> child when present on a host config;
    /// otherwise binds <paramref name="configuration"/> itself (already a section).
    /// </summary>
    public ClaviOidcConfiguration Configuration(
        IConfiguration configuration,
        string sectionName = ClaviOidcOptions.SectionName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return parent.Set(options =>
        {
            var section = configuration.GetSection(sectionName);
            if (section.Exists())
                section.Bind(options);
            else
                configuration.Bind(options);
        });
    }
}
