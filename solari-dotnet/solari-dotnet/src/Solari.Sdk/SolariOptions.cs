namespace Solari.Sdk;

/// <summary>
/// Configuration for <see cref="SolariClient"/>.
/// </summary>
public sealed class SolariOptions
{
    /// <summary>
    /// Your Solari API key, e.g. "slr_live_&lt;id&gt;_&lt;secret&gt;".
    /// Get one from https://console.getsolari.com.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The Solari API base address. Defaults to the production API.
    /// </summary>
    public Uri BaseAddress { get; set; } = new("https://api.getsolari.com");

    /// <summary>
    /// Overall timeout applied to individual HTTP requests made by the SDK.
    /// Long-lived resources (sandboxes/desktops) are unaffected - this only
    /// bounds a single create/get/delete call.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
