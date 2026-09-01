using System.Text.Json.Serialization;

namespace Solari.Sdk.Models;

/// <summary>Request body for POST /sessions (cloud browser).</summary>
public sealed record CreateSessionRequest(
    [property: JsonPropertyName("stealth")] bool? Stealth = null,
    [property: JsonPropertyName("recording")] bool? Recording = null,
    [property: JsonPropertyName("proxy")] string? Proxy = null,
    [property: JsonPropertyName("captcha")] bool? Captcha = null,
    [property: JsonPropertyName("profileId")] string? ProfileId = null,
    [property: JsonPropertyName("region")] string? Region = null
);

public sealed record StorageStateUrl(
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("expiresInSeconds")] int? ExpiresInSeconds
);

/// <summary>Response from POST /sessions and GET /sessions/{id}.</summary>
public sealed record SessionResponse(
    [property: JsonPropertyName("sessionId")] string SessionId,
    [property: JsonPropertyName("wsEndpoint")] string? WsEndpoint,
    [property: JsonPropertyName("cdpEndpoint")] string? CdpEndpoint,
    [property: JsonPropertyName("expiresAt")] DateTimeOffset? ExpiresAt,
    [property: JsonPropertyName("storageStateUrl")] StorageStateUrl? StorageStateUrl
);
