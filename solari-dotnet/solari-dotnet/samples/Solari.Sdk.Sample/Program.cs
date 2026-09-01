using Solari.Sdk;
using Solari.Sdk.Extensions;
using Solari.Sdk.Models;

// ---------------------------------------------------------------------------
// Solari Sample: "Untrusted Code Runner"
//
// A real use case for Solari from a .NET backend: an API that takes a small
// code snippet from a caller (think a "try it online" box, a coding-interview
// grader, or a tool an AI agent calls) and executes it inside a disposable
// Solari sandbox - an isolated Linux microVM - instead of running it on your
// own server. Each request gets its own sandbox, which is torn down
// afterwards no matter what happens.
// ---------------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSolari(options =>
{
    options.ApiKey = builder.Configuration["Solari:ApiKey"]
        ?? Environment.GetEnvironmentVariable("SOLARI_API_KEY")
        ?? throw new InvalidOperationException(
            "Set Solari:ApiKey (appsettings/user-secrets) or the SOLARI_API_KEY environment variable. " +
            "Get a key - and a free month with promo code STARTER1MO-MKY4BNDK - at https://console.getsolari.com.");
    options.RequestTimeout = TimeSpan.FromSeconds(45);
});

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "Solari.Sdk.Sample - Untrusted Code Runner",
    endpoints = new[] { "GET /health", "POST /api/run" }
}));

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/run", async (RunCodeRequest request, SolariClient solari, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Code))
    {
        return Results.BadRequest(new { error = "code is required" });
    }

    string cmd;
    string[] args;
    try
    {
        (cmd, args) = BuildCommand(request.Language, request.Code);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }

    SandboxResponse sandbox = await solari.Sandboxes.CreateAsync(
        new CreateSandboxRequest(Template: "base", Cpu: 1, MemMb: 512, TimeoutMs: 30_000),
        idempotencyKey: Guid.NewGuid().ToString(),
        cancellationToken: ct);

    try
    {
        ExecResponse result = await solari.Sandboxes.ExecAsync(
            sandbox.SandboxId,
            new ExecRequest(cmd, args, TimeoutMs: 10_000),
            ct);

        return Results.Ok(new RunCodeResponse(result.ExitCode, result.Stdout, result.Stderr));
    }
    catch (SolariApiException ex) when (ex.IsRetryable)
    {
        return Results.Problem(
            title: "Solari is temporarily unavailable",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    finally
    {
        // Always release the sandbox - untrusted code never lingers.
        await solari.Sandboxes.DeleteAsync(sandbox.SandboxId, CancellationToken.None);
    }
})
.WithName("RunCode");

app.Run();

static (string cmd, string[] args) BuildCommand(string? language, string code) => language?.ToLowerInvariant() switch
{
    "python" or "python3" => ("python3", new[] { "-c", code }),
    "javascript" or "node" => ("node", new[] { "-e", code }),
    "bash" or "sh" or null or "" => ("sh", new[] { "-c", code }),
    _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Supported languages: python, javascript, bash"),
};

public sealed record RunCodeRequest(string Code, string? Language);

public sealed record RunCodeResponse(int ExitCode, string Stdout, string Stderr);
