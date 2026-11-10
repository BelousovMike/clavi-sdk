using Clavi.Sdk.Admin;
using Clavi.Sdk.Core;
using Xunit;

namespace Clavi.Sdk.Smoke;

public sealed class AdminSmokeTests
{
    [Fact]
    public async Task Login_And_List_Users()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("CLAVI_SDK_SMOKE"), "1", StringComparison.Ordinal))
            return;

        var baseUrl = Environment.GetEnvironmentVariable("CLAVI_BASE_URL") ?? "https://clavi.clavi.local";
        var host = Environment.GetEnvironmentVariable("CLAVI_TENANT_HOST") ?? "clavi.clavi.local";
        var user = Environment.GetEnvironmentVariable("CLAVI_E2E_ADMIN_USERNAME") ?? "admin";
        var password = Environment.GetEnvironmentVariable("CLAVI_E2E_ADMIN_PASSWORD")
                       ?? throw new InvalidOperationException("CLAVI_E2E_ADMIN_PASSWORD required for smoke");

        await using var client = new ClaviAdminClient(new ClaviClientOptions
        {
            BaseUrl = new Uri(baseUrl.TrimEnd('/') + "/"),
            TenantHost = host,
            AcceptAnyServerCertificate = true
        });

        await client.AuthAsync(user, password);
        var page = await client.Users.FindAsync(page: 1, pageSize: 10);
        Assert.NotNull(page);
        Assert.NotNull(page.Items);
        Assert.True(page.PageSize <= 100);
    }
}
