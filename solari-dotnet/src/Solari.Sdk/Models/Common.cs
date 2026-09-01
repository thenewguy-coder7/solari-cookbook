using System.Text.Json.Serialization;

namespace Solari.Sdk.Models;

/// <summary>Generic "{ ok: true }" acknowledgement returned by several endpoints.</summary>
public sealed record OkResponse(
    [property: JsonPropertyName("ok")] bool Ok
);

/// <summary>Lifecycle behavior for long-lived resources (desktops).</summary>
public sealed record LifecycleOptions(
    [property: JsonPropertyName("onTimeout")] string? OnTimeout = null,
    [property: JsonPropertyName("autoResume")] bool? AutoResume = null
);
