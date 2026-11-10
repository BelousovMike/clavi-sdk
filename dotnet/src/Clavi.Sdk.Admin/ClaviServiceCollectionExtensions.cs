using Clavi.Sdk.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Clavi.Sdk.Admin;

/// <summary>
/// Registers a singleton <see cref="ClaviAdminClient"/> from configuration
/// (section usually named <c>Clavi</c>) or from a configure delegate.
/// </summary>
public static class ClaviServiceCollectionExtensions
{
    public static IServiceCollection AddClaviAdmin(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.Configure<ClaviClientOptions>(configuration);
        return AddClaviAdminCore(services);
    }

    public static IServiceCollection AddClaviAdmin(this IServiceCollection services, Action<ClaviClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        services.Configure(configure);
        return AddClaviAdminCore(services);
    }

    private static IServiceCollection AddClaviAdminCore(IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ClaviClientOptions>>().Value;
            return new ClaviAdminClient(options);
        });
        return services;
    }
}
