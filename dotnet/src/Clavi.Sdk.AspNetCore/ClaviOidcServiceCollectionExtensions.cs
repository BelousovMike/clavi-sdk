using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Clavi.Sdk.AspNetCore;

/// <summary>
/// Wires cookie + OpenID Connect against Clavi (PKCE, optional role claim mapping).
/// </summary>
/// <example>
/// appsettings.json <c>Clavi</c> section only:
/// <code>builder.Services.AddClaviOpenIdConnect();</code>
/// Fluent + config (Serilog-style):
/// <code>
/// builder.Services.AddClaviOpenIdConnect(c => c
///     .ReadFrom.Configuration(builder.Configuration)
///     .AcceptAnyServerCertificate(builder.Environment.IsDevelopment()));
/// </code>
/// </example>
public static class ClaviOidcServiceCollectionExtensions
{
    /// <summary>Bind the default <c>Clavi</c> section from host configuration.</summary>
    public static AuthenticationBuilder AddClaviOpenIdConnect(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<ClaviOidcOptions>()
            .BindConfiguration(ClaviOidcOptions.SectionName);
        return AddClaviOpenIdConnectCore(services);
    }

    /// <summary>
    /// Bind from configuration. Pass host <see cref="IConfiguration"/> (uses section <c>Clavi</c>)
    /// or an already-selected section such as <c>GetSection("Clavi")</c>.
    /// </summary>
    public static AuthenticationBuilder AddClaviOpenIdConnect(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = ClaviOidcOptions.SectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(sectionName);
        services.Configure<ClaviOidcOptions>(section.Exists() ? section : configuration);
        return AddClaviOpenIdConnectCore(services);
    }

    /// <summary>Configure options in code (Options pattern).</summary>
    public static AuthenticationBuilder AddClaviOpenIdConnect(
        this IServiceCollection services,
        Action<ClaviOidcOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        services.Configure(configure);
        return AddClaviOpenIdConnectCore(services);
    }

    /// <summary>Serilog-style fluent configuration (can combine <c>ReadFrom.Configuration</c> + overrides).</summary>
    public static AuthenticationBuilder AddClaviOpenIdConnect(
        this IServiceCollection services,
        Action<ClaviOidcConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var fluent = new ClaviOidcConfiguration();
        configure(fluent);
        services.Configure<ClaviOidcOptions>(fluent.ApplyTo);
        return AddClaviOpenIdConnectCore(services);
    }

    private static AuthenticationBuilder AddClaviOpenIdConnectCore(IServiceCollection services)
    {
        services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ClaviOpenIdConnectConfigureOptions>();

        return services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect();
    }

    private sealed class ClaviOpenIdConnectConfigureOptions(IOptions<ClaviOidcOptions> claviOptions)
        : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        public void Configure(string? name, OpenIdConnectOptions options)
        {
            if (!string.Equals(name, OpenIdConnectDefaults.AuthenticationScheme, StringComparison.Ordinal))
                return;
            Configure(options);
        }

        public void Configure(OpenIdConnectOptions options) => Apply(options, claviOptions.Value);
    }

    internal static void Apply(OpenIdConnectOptions options, ClaviOidcOptions clavi)
    {
        clavi.Validate();

        options.Authority = clavi.Authority.TrimEnd('/');
        options.ClientId = clavi.ClientId;
        options.ClientSecret = clavi.ClientSecret;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.CallbackPath = clavi.CallbackPath;
        options.SignedOutCallbackPath = clavi.SignedOutCallbackPath;
        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

        options.Scope.Clear();
        foreach (var scope in clavi.Scopes)
            options.Scope.Add(scope);

#if NET9_0_OR_GREATER
        if (clavi.DisablePushedAuthorization)
            options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
#endif

        if (clavi.AcceptAnyServerCertificate)
            options.BackchannelHttpHandler = LabTlsHandler();

        if (clavi.MapRoles)
            ClaviOidcRoleClaims.Attach(options);
    }

    private static HttpClientHandler LabTlsHandler() => new()
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
}
