using System.Net.Http.Json;
using Solari.Sdk.Internal;
using Solari.Sdk.Models;

namespace Solari.Sdk;

/// <summary>
/// Code sandboxes ("/sandboxes") - isolated Linux microVMs for running
/// commands and code without touching your own infrastructure.
/// </summary>
public sealed class SandboxesClient
{
    private readonly SolariHttpCore _core;

    internal SandboxesClient(SolariHttpCore core) => _core = core;

    /// <summary>Creates a new sandbox. POST /sandboxes.</summary>
    /// <param name="request">Sandbox configuration (template, resources, timeout). Defaults to a base template.</param>
    /// <param name="idempotencyKey">
    /// Optional idempotency key so a retried create doesn't spin up a
    /// duplicate sandbox. The docs recommend a fresh GUID per logical
    /// creation attempt (not per HTTP retry).
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the create request.</param>
    public Task<SandboxResponse> CreateAsync(
        CreateSandboxRequest? request = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
        => _core.SendAsync<SandboxResponse>(
            HttpMethod.Post,
            "/sandboxes",
            request ?? new CreateSandboxRequest(),
            idempotencyKey,
            cancellationToken);

    /// <summary>
    /// Runs a single command in the sandbox and waits for it to finish.
    /// POST /sandboxes/{id}/exec. Good for short-lived commands; for
    /// long/interactive sessions use the sandbox's control WebSocket
    /// instead (not covered by this SDK yet).
    /// </summary>
    public Task<ExecResponse> ExecAsync(
        string sandboxId,
        ExecRequest request,
        CancellationToken cancellationToken = default)
        => _core.SendAsync<ExecResponse>(
            HttpMethod.Post,
            $"/sandboxes/{Uri.EscapeDataString(sandboxId)}/exec",
            request,
            cancellationToken: cancellationToken);

    /// <summary>Mints a short-lived signed URL for uploading a file into the sandbox. GET /sandboxes/{id}/files/upload-url.</summary>
    public Task<SignedFileUrlResponse> GetUploadUrlAsync(
        string sandboxId,
        string path,
        CancellationToken cancellationToken = default)
        => _core.SendAsync<SignedFileUrlResponse>(
            HttpMethod.Get,
            $"/sandboxes/{Uri.EscapeDataString(sandboxId)}/files/upload-url?path={Uri.EscapeDataString(path)}",
            cancellationToken: cancellationToken);

    /// <summary>Mints a short-lived signed URL for downloading a file from the sandbox. GET /sandboxes/{id}/files/download-url.</summary>
    public Task<SignedFileUrlResponse> GetDownloadUrlAsync(
        string sandboxId,
        string path,
        CancellationToken cancellationToken = default)
        => _core.SendAsync<SignedFileUrlResponse>(
            HttpMethod.Get,
            $"/sandboxes/{Uri.EscapeDataString(sandboxId)}/files/download-url?path={Uri.EscapeDataString(path)}",
            cancellationToken: cancellationToken);

    /// <summary>
    /// Convenience wrapper: mints an upload URL for <paramref name="path"/> and
    /// PUTs <paramref name="content"/> to it. Signed URLs need no Authorization header.
    /// </summary>
    public async Task<FileUploadResult> UploadFileAsync(
        string sandboxId,
        string path,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var signed = await GetUploadUrlAsync(sandboxId, path, cancellationToken).ConfigureAwait(false);
        using var streamContent = new StreamContent(content);
        using var response = await _core.RawClient.PutAsync(signed.Url, streamContent, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FileUploadResult>(SolariHttpCore.JsonOptions, cancellationToken).ConfigureAwait(false)
               ?? new FileUploadResult(true, path, content.Length);
    }

    /// <summary>
    /// Convenience wrapper: mints a download URL for <paramref name="path"/> and
    /// GETs its bytes as a stream.
    /// </summary>
    public async Task<Stream> DownloadFileAsync(
        string sandboxId,
        string path,
        CancellationToken cancellationToken = default)
    {
        var signed = await GetDownloadUrlAsync(sandboxId, path, cancellationToken).ConfigureAwait(false);
        return await _core.RawClient.GetStreamAsync(signed.Url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Terminates a sandbox. DELETE /sandboxes/{id}. Deleting an already-gone sandbox still succeeds.</summary>
    public Task<OkResponse> DeleteAsync(string sandboxId, CancellationToken cancellationToken = default)
        => _core.SendAsync<OkResponse>(
            HttpMethod.Delete,
            $"/sandboxes/{Uri.EscapeDataString(sandboxId)}",
            cancellationToken: cancellationToken);
}
