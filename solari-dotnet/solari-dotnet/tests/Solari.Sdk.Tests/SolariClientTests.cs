using System.Net;
using Solari.Sdk.Models;

namespace Solari.Sdk.Tests;

internal static class SolariClientTests
{
    private static SolariClient MakeClient(FakeHttpMessageHandler handler) =>
        new(new HttpClient(handler), new SolariOptions { ApiKey = "slr_live_test_secret" });

    public static void CreateSandbox_Sends_Bearer_Auth_And_Correct_Body()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """
            { "sandboxId": "pool-sbx-sta:vm_1:org_1.abc", "kind": "sandbox", "controlUrl": "wss://x", "expiresAt": "2026-07-16T14:00:00.000Z" }
            """);
        var client = MakeClient(handler);

        var result = client.Sandboxes.CreateAsync(
            new CreateSandboxRequest(Template: "base", Cpu: 4, MemMb: 8192),
            idempotencyKey: "idem-123").GetAwaiter().GetResult();

        Assert.Equal("pool-sbx-sta:vm_1:org_1.abc", result.SandboxId);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("/sandboxes", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("slr_live_test_secret", handler.LastRequest.Headers.Authorization.Parameter);
        Assert.Equal("idem-123", handler.LastRequest.Headers.GetValues("Idempotency-Key").Single());
        Assert.Contains("\"template\":\"base\"", handler.LastRequestBody!);
        Assert.Contains("\"cpu\":4", handler.LastRequestBody!);
    }

    public static void Exec_Parses_Stdout_Stderr_ExitCode()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """
            { "exitCode": 0, "stdout": "hello\n", "stderr": "" }
            """);
        var client = MakeClient(handler);

        var result = client.Sandboxes.ExecAsync(
            "pool-sbx-sta:vm_1:org_1.abc",
            new ExecRequest("sh", new[] { "-c", "echo hello" })).GetAwaiter().GetResult();

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("hello\n", result.Stdout);
        // sandbox id contains ':' and '.' and must be percent-encoded in the path.
        Assert.Contains("/sandboxes/pool-sbx-sta%3Avm_1%3Aorg_1.abc/exec", handler.LastRequest!.RequestUri!.PathAndQuery);
    }

    public static void NonSuccess_Status_Throws_SolariApiException_With_Parsed_Message()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.TooManyRequests, """
            { "error": "concurrency limit reached" }
            """);
        var client = MakeClient(handler);

        var ex = Assert.ThrowsAsync<SolariApiException>(() =>
            client.Sandboxes.CreateAsync());
        ex.GetAwaiter().GetResult();
    }

    public static void ServiceUnavailable_Is_Marked_Retryable_But_TooManyRequests_Is_Not()
    {
        var unavailable = new FakeHttpMessageHandler(HttpStatusCode.ServiceUnavailable, "{}");
        var busy = new FakeHttpMessageHandler(HttpStatusCode.TooManyRequests, "{}");

        SolariApiException? unavailableEx = null;
        try
        {
            MakeClient(unavailable).Sandboxes.CreateAsync().GetAwaiter().GetResult();
        }
        catch (SolariApiException ex)
        {
            unavailableEx = ex;
        }

        SolariApiException? busyEx = null;
        try
        {
            MakeClient(busy).Sandboxes.CreateAsync().GetAwaiter().GetResult();
        }
        catch (SolariApiException ex)
        {
            busyEx = ex;
        }

        Assert.True(unavailableEx is { IsRetryable: true }, "503 should be retryable");
        Assert.True(busyEx is { IsRetryable: false }, "429 should NOT be reported retryable");
    }

    public static void Sessions_Create_Defaults_To_Empty_Body_When_No_Request_Given()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Created, """
            { "sessionId": "pool-7f3a:1:org_1:123.zzz", "wsEndpoint": "wss://x", "cdpEndpoint": "wss://y", "expiresAt": "2026-07-17T09:00:00.000Z", "storageStateUrl": { "url": null, "expiresInSeconds": 60 } }
            """);
        var client = MakeClient(handler);

        var result = client.Sessions.CreateAsync().GetAwaiter().GetResult();

        Assert.Equal("pool-7f3a:1:org_1:123.zzz", result.SessionId);
        Assert.Equal("/sessions", handler.LastRequest!.RequestUri!.AbsolutePath);
    }

    public static void Desktops_Pause_Resume_Hit_Correct_Paths()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """
            { "sessionId": "pool-desktop-sta:vm_1:org_1.abc", "status": "paused" }
            """);
        var client = MakeClient(handler);

        client.Desktops.PauseAsync("pool-desktop-sta:vm_1:org_1.abc").GetAwaiter().GetResult();
        Assert.Contains("/desktops/pool-desktop-sta%3Avm_1%3Aorg_1.abc/pause", handler.LastRequest!.RequestUri!.PathAndQuery);
    }

    public static void Constructor_Throws_When_ApiKey_Missing()
    {
        try
        {
            _ = new SolariClient(new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}")), new SolariOptions());
            throw new Exception("Expected ArgumentException for missing ApiKey.");
        }
        catch (ArgumentException)
        {
            // expected
        }
    }
}
