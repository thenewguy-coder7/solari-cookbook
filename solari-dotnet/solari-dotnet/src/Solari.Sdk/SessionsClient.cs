using Solari.Sdk.Internal;
using Solari.Sdk.Models;

namespace Solari.Sdk;

/// <summary>
/// Cloud browser sessions ("/sessions"). A session gives you a CDP endpoint
/// you connect to with Playwright/Puppeteer, e.g.:
/// <c>chromium.connectOverCDP(session.CdpEndpoint)</c>.
/// </summary>
public sealed class SessionsClient
{
    private readonly SolariHttpCore _core;

    internal SessionsClient(SolariHttpCore core) => _core = core;

    /// <summary>Creates a new cloud browser session. POST /sessions.</summary>
    public Task<SessionResponse> CreateAsync(
        CreateSessionRequest? request = null,
        CancellationToken cancellationToken = default)
        => _core.SendAsync<SessionResponse>(HttpMethod.Post, "/sessions", request ?? new CreateSessionRequest(), cancellationToken: cancellationToken);

    /// <summary>
    /// Releases a session. DELETE /sessions/{id}. Per the docs this is
    /// fire-and-forget: the gateway returns immediately while teardown
    /// happens asynchronously.
    /// </summary>
    public Task ReleaseAsync(string sessionId, CancellationToken cancellationToken = default)
        => _core.SendAsync(HttpMethod.Delete, $"/sessions/{Uri.EscapeDataString(sessionId)}", cancellationToken: cancellationToken);
}
