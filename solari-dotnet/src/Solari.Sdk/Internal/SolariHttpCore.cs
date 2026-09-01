using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Solari.Sdk.Internal;

/// <summary>
/// Shared request/response plumbing used by all Solari resource clients:
/// auth header, JSON (de)serialization, and translating non-success
/// responses into <see cref="SolariApiException"/>.
/// </summary>
internal sealed class SolariHttpCore
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public SolariHttpCore(HttpClient http, SolariOptions options)
    {
        _http = http;
        _http.BaseAddress ??= options.BaseAddress;
        if (_http.Timeout == Timeout.InfiniteTimeSpan || _http.Timeout == TimeSpan.FromSeconds(100))
        {
            _http.Timeout = options.RequestTimeout;
        }

        if (!string.IsNullOrEmpty(options.ApiKey))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options.ApiKey);
        }
    }

    public async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string requestUri,
        object? body = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        }

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw BuildException(response, raw);
        }

        if (typeof(TResponse) == typeof(EmptyResponse) || string.IsNullOrWhiteSpace(raw))
        {
            return default!;
        }

        return JsonSerializer.Deserialize<TResponse>(raw, JsonOptions)
               ?? throw new SolariApiException(response.StatusCode, raw, "Solari API returned an empty or unparsable body.");
    }

    public async Task SendAsync(
        HttpMethod method,
        string requestUri,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw BuildException(response, raw);
        }
    }

    /// <summary>Exposed for resource clients that need the raw HttpClient (e.g. streaming a signed URL download).</summary>
    public HttpClient RawClient => _http;

    private static SolariApiException BuildException(HttpResponseMessage response, string raw)
    {
        string message = $"Solari API request failed with {(int)response.StatusCode} {response.StatusCode}.";
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String)
            {
                message += $" {err.GetString()}";
            }
            else if (doc.RootElement.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
            {
                message += $" {msg.GetString()}";
            }
        }
        catch (JsonException)
        {
            // Body wasn't JSON (or was empty) - fall back to the plain status line.
        }

        return new SolariApiException(response.StatusCode, raw, message);
    }
}

/// <summary>Marker type used internally to request "don't try to deserialize a body".</summary>
internal sealed class EmptyResponse
{
}
