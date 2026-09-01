using Solari.Sdk.Internal;

namespace Solari.Sdk;

/// <summary>
/// Entry point for the Solari .NET SDK - a thin, dependency-free client for
/// Solari's cloud browsers, code sandboxes, and desktop VMs
/// (https://docs.getsolari.com). Solari doesn't ship an official .NET SDK,
/// so this talks to the plain REST API directly over <see cref="HttpClient"/>.
///
/// <code>
/// var client = new SolariClient(new SolariOptions { ApiKey = apiKey });
/// var sandbox = await client.Sandboxes.CreateAsync();
/// var result = await client.Sandboxes.ExecAsync(sandbox.SandboxId, new ExecRequest("echo", new[] { "hello" }));
/// await client.Sandboxes.DeleteAsync(sandbox.SandboxId);
/// </code>
///
/// In an ASP.NET Core app, prefer registering it via
/// <see cref="Extensions.SolariServiceCollectionExtensions.AddSolari"/> and
/// injecting <see cref="SolariClient"/> so the underlying HttpClient is
/// pooled correctly by <c>IHttpClientFactory</c>.
/// </summary>
public sealed class SolariClient
{
    /// <summary>Cloud browser sessions.</summary>
    public SessionsClient Sessions { get; }

    /// <summary>Code sandboxes (isolated Linux microVMs).</summary>
    public SandboxesClient Sandboxes { get; }

    /// <summary>Full desktop VMs with a VNC stream and computer-use control channel.</summary>
    public DesktopsClient Desktops { get; }

    /// <summary>
    /// Creates a standalone client with its own <see cref="HttpClient"/>. Fine for
    /// scripts/console apps; in ASP.NET Core prefer the DI-registered version so
    /// sockets get pooled by <c>IHttpClientFactory</c>.
    /// </summary>
    public SolariClient(SolariOptions options)
        : this(new HttpClient(), options)
    {
    }

    /// <summary>Creates a client around an existing <see cref="HttpClient"/> (typically supplied by DI).</summary>
    public SolariClient(HttpClient httpClient, SolariOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new ArgumentException("SolariOptions.ApiKey must be set. Get a key from https://console.getsolari.com.", nameof(options));
        }

        var core = new SolariHttpCore(httpClient, options);
        Sessions = new SessionsClient(core);
        Sandboxes = new SandboxesClient(core);
        Desktops = new DesktopsClient(core);
    }
}
