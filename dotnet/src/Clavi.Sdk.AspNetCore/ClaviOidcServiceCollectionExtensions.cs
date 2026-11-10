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
/// <code>
/// builder.Services.AddClaviOpenIdConnect(builder.Configuration.GetSection("Clavi"));
/// // app policies stay in the host:
/// builder.Services.AddAuthorization(o => o.AddPolicy("catalog.write", p => p.RequireRole("acme.catalog.write")));
/// </code>
/// </example>
public static class ClaviOidcServiceCollectionExtensions
{
    /// <summary>Bind options from a config section (usually <c>Clavi</c>).</summary>
    public static AuthenticationBuilder AddClaviOpenIdConnect(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.Configure<ClaviOidcOptions>(configuration);
        return AddClaviOpenIdConnectCore(services);
    }

    /// <summary>Configure options in code.</summary>
    public static AuthenticationBuilder AddClaviOpenIdConnect(
        this IServiceCollection services,
        Action<ClaviOidcOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        services.Configure(configure);
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

    /// <summary>Applies <see cref="ClaviOidcOptions"/> onto the default OIDC scheme.</summary>
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
