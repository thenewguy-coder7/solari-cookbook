using System.Text.Json.Serialization;

namespace Solari.Sdk.Models;

/// <summary>Request body for POST /desktops.</summary>
public sealed record CreateDesktopRequest(
    [property: JsonPropertyName("template")] string Template = "default",
    [property: JsonPropertyName("resolution")] string? Resolution = null,
    [property: JsonPropertyName("cpu")] int? Cpu = null,
    [property: JsonPropertyName("memMb")] int? MemMb = null,
    [property: JsonPropertyName("timeoutMs")] long? TimeoutMs = null,
    [property: JsonPropertyName("lifecycle")] LifecycleOptions? Lifecycle = null,
    [property: JsonPropertyName("metadata")] Dictionary<string, string>? Metadata = null
);

/// <summary>Response from POST /desktops.</summary>
public sealed record DesktopResponse(
    [property: JsonPropertyName("sessionId")] string SessionId,
    [property: JsonPropertyName("streamUrl")] string? StreamUrl,
    [property: JsonPropertyName("controlUrl")] string? ControlUrl,
    [property: JsonPropertyName("expiresAt")] DateTimeOffset? ExpiresAt
);

/// <summary>Response from GET /desktops/{id}, POST /desktops/{id}/pause, POST /desktops/{id}/resume.</summary>
public sealed record DesktopStatusResponse(
    [property: JsonPropertyName("sessionId")] string SessionId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("expiresAt")] DateTimeOffset? ExpiresAt,
    [property: JsonPropertyName("orgId")] string? OrgId
);
