using System.Net;

namespace Clavi.Sdk.Core;

public sealed class CookieSessionHandler(CookieContainer cookies) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var uri = request.RequestUri
                  ?? throw new InvalidOperationException("RequestUri is required.");

        var header = cookies.GetCookieHeader(uri);
        if (!string.IsNullOrEmpty(header) && !request.Headers.Contains("Cookie"))
            request.Headers.TryAddWithoutValidation("Cookie", header);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            foreach (var value in setCookies)
                cookies.SetCookies(uri, value);
        }

        return response;
    }
}
