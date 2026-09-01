using Solari.Sdk.Internal;
using Solari.Sdk.Models;

namespace Solari.Sdk;

/// <summary>
/// Full Linux desktop VMs ("/desktops") with a VNC stream and a
/// computer-use control channel - useful for agents that need to drive a GUI.
/// </summary>
public sealed class DesktopsClient
{
    private readonly SolariHttpCore _core;

    internal DesktopsClient(SolariHttpCore core) => _core = core;

    /// <summary>Creates a new desktop VM. POST /desktops.</summary>
    public Task<DesktopResponse> CreateAsync(
        CreateDesktopRequest? request = null,
        CancellationToken cancellationToken = default)
        => _core.SendAsync<DesktopResponse>(HttpMethod.Post, "/desktops", request ?? new CreateDesktopRequest(), cancellationToken: cancellationToken);

    /// <summary>Gets the current status of a desktop. GET /desktops/{id}.</summary>
    public Task<DesktopStatusResponse> GetAsync(string sessionId, CancellationToken cancellationToken = default)
        => _core.SendAsync<DesktopStatusResponse>(HttpMethod.Get, $"/desktops/{Uri.EscapeDataString(sessionId)}", cancellationToken: cancellationToken);

    /// <summary>Pauses a desktop so it stops billing/consuming capacity while preserving state. POST /desktops/{id}/pause.</summary>
    public Task<DesktopStatusResponse> PauseAsync(string sessionId, CancellationToken cancellationToken = default)
        => _core.SendAsync<DesktopStatusResponse>(HttpMethod.Post, $"/desktops/{Uri.EscapeDataString(sessionId)}/pause", cancellationToken: cancellationToken);

    /// <summary>Resumes a paused desktop. Prefers the same host for a fast local restore. POST /desktops/{id}/resume.</summary>
    public Task<DesktopStatusResponse> ResumeAsync(string sessionId, CancellationToken cancellationToken = default)
        => _core.SendAsync<DesktopStatusResponse>(HttpMethod.Post, $"/desktops/{Uri.EscapeDataString(sessionId)}/resume", cancellationToken: cancellationToken);

    /// <summary>Terminates a desktop. DELETE /desktops/{id}.</summary>
    public Task<OkResponse> DeleteAsync(string sessionId, CancellationToken cancellationToken = default)
        => _core.SendAsync<OkResponse>(HttpMethod.Delete, $"/desktops/{Uri.EscapeDataString(sessionId)}", cancellationToken: cancellationToken);
}
