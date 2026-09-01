using System.Text.Json.Serialization;

namespace Solari.Sdk.Models;

/// <summary>Request body for POST /sandboxes.</summary>
public sealed record CreateSandboxRequest(
    [property: JsonPropertyName("template")] string Template = "base",
    [property: JsonPropertyName("kind")] string Kind = "sandbox",
    [property: JsonPropertyName("cpu")] int? Cpu = null,
    [property: JsonPropertyName("memMb")] int? MemMb = null,
    [property: JsonPropertyName("timeoutMs")] long? TimeoutMs = null,
    [property: JsonPropertyName("metadata")] Dictionary<string, string>? Metadata = null
);

/// <summary>Response from POST /sandboxes.</summary>
public sealed record SandboxResponse(
    [property: JsonPropertyName("sandboxId")] string SandboxId,
    [property: JsonPropertyName("kind")] string? Kind,
    [property: JsonPropertyName("controlUrl")] string? ControlUrl,
    [property: JsonPropertyName("expiresAt")] DateTimeOffset? ExpiresAt
);

/// <summary>Request body for POST /sandboxes/{id}/exec (one-shot command execution).</summary>
public sealed record ExecRequest(
    [property: JsonPropertyName("cmd")] string Cmd,
    [property: JsonPropertyName("args")] IReadOnlyList<string>? Args = null,
    [property: JsonPropertyName("cwd")] string? Cwd = null,
    [property: JsonPropertyName("env")] Dictionary<string, string>? Env = null,
    [property: JsonPropertyName("timeoutMs")] long? TimeoutMs = null
);

/// <summary>Response from POST /sandboxes/{id}/exec.</summary>
public sealed record ExecResponse(
    [property: JsonPropertyName("exitCode")] int ExitCode,
    [property: JsonPropertyName("stdout")] string Stdout,
    [property: JsonPropertyName("stderr")] string Stderr
);

/// <summary>Response from the upload-url / download-url minting endpoints.</summary>
public sealed record SignedFileUrlResponse(
    [property: JsonPropertyName("url")] string Url
);

/// <summary>Response after a PUT to a signed upload URL.</summary>
public sealed record FileUploadResult(
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("bytes")] long Bytes
);
