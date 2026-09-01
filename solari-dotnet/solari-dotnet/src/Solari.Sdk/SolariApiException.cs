using System.Net;

namespace Solari.Sdk;

/// <summary>
/// Thrown when the Solari API returns a non-success status code. Wraps the
/// documented error semantics so callers can branch on <see cref="StatusCode"/>
/// instead of parsing strings:
///
///   400 - malformed request body
///   401 - missing/invalid bearer token
///   402 - feature requires a paid plan or insufficient credit
///   404 - unknown session id, or cross-org access attempt
///   429 - concurrency limit reached (NOT retryable - pause/kill something first)
///   502 - host rejected the operation (retryable)
///   503 - no available capacity (retryable with backoff)
/// </summary>
public sealed class SolariApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    /// <summary>Raw response body returned by the API, if any.</summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// True for errors the docs describe as safe to retry (502/503).
    /// 429 is deliberately excluded - the API says it is not retryable
    /// until the caller frees up capacity themselves.
    /// </summary>
    public bool IsRetryable => StatusCode is HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable;

    public SolariApiException(HttpStatusCode statusCode, string? responseBody, string message)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
