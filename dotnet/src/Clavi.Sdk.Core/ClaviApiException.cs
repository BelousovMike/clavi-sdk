using System.Net;
using System.Text.Json;

namespace Clavi.Sdk.Core;

public sealed class ClaviApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? Code { get; }
    public string? Body { get; }

    public ClaviApiException(HttpStatusCode statusCode, string? code, string? body)
        : base(Format(statusCode, code, body))
    {
        StatusCode = statusCode;
        Code = code;
        Body = body;
    }

    public static async Task<ClaviApiException> FromResponseAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        string? code = null;
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("code", out var c))
                    code = c.GetString();
                else if (doc.RootElement.TryGetProperty("extensions", out var ext)
                         && ext.TryGetProperty("code", out var ec))
                    code = ec.GetString();
            }
            catch (JsonException)
            {
            }
        }

        return new ClaviApiException(response.StatusCode, code, body);
    }

    private static string Format(HttpStatusCode status, string? code, string? body)
        => code is null
            ? $"Clavi HTTP {(int)status}: {Truncate(body)}"
            : $"Clavi HTTP {(int)status} [{code}]: {Truncate(body)}";

    private static string Truncate(string? body)
        => string.IsNullOrEmpty(body) ? "" : body.Length <= 200 ? body : body[..200];
}
