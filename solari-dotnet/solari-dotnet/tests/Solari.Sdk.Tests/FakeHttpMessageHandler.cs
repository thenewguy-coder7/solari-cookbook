using System.Net;
using System.Text;

namespace Solari.Sdk.Tests;

/// <summary>
/// A tiny in-memory HTTP handler so tests can assert on the exact request the
/// SDK sends (method, path, body) and script canned responses, without any
/// network access or mocking library.
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    private readonly HttpStatusCode _statusCode;
    private readonly string _responseBody;
    private readonly string _contentType;

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string responseBody, string contentType = "application/json")
    {
        _statusCode = statusCode;
        _responseBody = responseBody;
        _contentType = contentType;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestBody = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, Encoding.UTF8, _contentType),
        };
    }
}
