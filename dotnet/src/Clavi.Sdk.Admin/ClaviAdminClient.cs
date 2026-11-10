using System.Net;
using Clavi.Sdk.Admin.Apis;
using Clavi.Sdk.Admin.Models;
using Clavi.Sdk.Admin.Resources;
using Clavi.Sdk.Core;
using Microsoft.Extensions.Options;
using Refit;

namespace Clavi.Sdk.Admin;

/// <summary>
/// Entry point for the Admin API. Create one client per session, call
/// <see cref="AuthAsync"/>, then use <see cref="Users"/>, <see cref="Groups"/>, and the rest.
/// Cookies and Host/Origin headers are handled for you (Refit under the hood).
/// </summary>
/// <example>
/// <code>
/// await using var client = new ClaviAdminClient(new ClaviClientOptions {
///     BaseUrl = new Uri("https://clavi.example/")
/// });
/// await client.AuthAsync("admin", password);
/// var page = await client.Users.FindAsync(1, 20);
/// </code>
/// </example>
public sealed class ClaviAdminClient : IAsyncDisposable, IDisposable
{
    private readonly HttpClient _http;
    private readonly IAuthApi _auth;

    public ClaviAdminClient(ClaviClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        _http = CreateHttpClient(options);
        var settings = ClaviRefit.Settings;
        _auth = RestService.For<IAuthApi>(_http, settings);
        Users = new UsersResource(RestService.For<IUsersApi>(_http, settings));
        Groups = new GroupsResource(RestService.For<IGroupsApi>(_http, settings));
        Roles = new RolesResource(RestService.For<IRolesApi>(_http, settings));
        Clients = new ClientsResource(RestService.For<IClientsApi>(_http, settings));
        Audit = new AuditResource(RestService.For<IAuditApi>(_http, settings));
        Tenants = new TenantsResource(RestService.For<ITenantsApi>(_http, settings));
        Security = new SecurityResource(RestService.For<ISecurityApi>(_http, settings));
        Me = new MeResource(RestService.For<IMeApi>(_http, settings));
    }

    public UsersResource Users { get; }
    public GroupsResource Groups { get; }
    public RolesResource Roles { get; }
    public ClientsResource Clients { get; }
    public AuditResource Audit { get; }
    public TenantsResource Tenants { get; }
    public SecurityResource Security { get; }
    public MeResource Me { get; }

    /// <summary>BFF login; keeps the session cookie on this client.</summary>
    public Task AuthAsync(string username, string password, CancellationToken cancellationToken = default)
        => _auth.LoginAsync(new LoginRequest(username, password), cancellationToken);

    /// <summary>Ends the BFF session.</summary>
    public Task LogoutAsync(CancellationToken cancellationToken = default)
        => _auth.LogoutAsync(cancellationToken);

    /// <summary>Builds the shared HttpClient pipeline (cookies + Host/Origin).</summary>
    public static HttpClient CreateHttpClient(ClaviClientOptions options)
    {
        options.Validate();
        var cookies = new CookieContainer();
        HttpMessageHandler pipeline = new HttpClientHandler
        {
            UseCookies = false,
            ServerCertificateCustomValidationCallback = options.AcceptAnyServerCertificate
                ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                : null
        };
        pipeline = new CookieSessionHandler(cookies) { InnerHandler = pipeline };
        pipeline = new HostOriginHandler(Options.Create(options)) { InnerHandler = pipeline };
        return new HttpClient(pipeline) { BaseAddress = options.BaseUrl };
    }

    public void Dispose() => _http.Dispose();

    public ValueTask DisposeAsync()
    {
        _http.Dispose();
        return ValueTask.CompletedTask;
    }
}

internal static class ClaviRefit
{
    public static readonly RefitSettings Settings = new()
    {
        ContentSerializer = new SystemTextJsonContentSerializer(ClaviJson.Options)
    };
}
