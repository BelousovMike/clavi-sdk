using Microsoft.Extensions.Options;

namespace Clavi.Sdk.Core;

public sealed class HostOriginHandler(IOptions<ClaviClientOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var o = options.Value;
        request.Headers.Host = o.ResolveTenantHost();
        if (!request.Headers.Contains("Origin"))
            request.Headers.TryAddWithoutValidation("Origin", o.ResolveOrigin());
        return base.SendAsync(request, cancellationToken);
    }
}
