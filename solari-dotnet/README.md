# Solari.Sdk — an unofficial C# / .NET client for Solari

[Solari](https://getsolari.com) gives you cloud browsers, code sandboxes, and
desktop VMs behind a single API key — but as of today it only ships official
SDKs for **TypeScript, Python, Go, Rust, and C++**. There's no .NET SDK.

This fills that gap. `Solari.Sdk` is a small, dependency-free C# client for
Solari's REST API ([docs.getsolari.com](https://docs.getsolari.com)), built
so any ASP.NET Core (or plain console/worker) app can spin up a browser,
sandbox, or desktop VM with a couple of `await` calls instead of hand-rolling
`HttpClient` code against the raw API every time.

> Built for Harry Chow's [Solari SWE-intern challenge](https://x.com/harrychow_)
> — "fork the repo, build a real use case, ship it." This *is* the use case:
> the .NET ecosystem gets first-class Solari support.

## Why this is a real use case, not a toy

- **It's a genuine gap.** The docs explicitly note "new languages only need
  to speak the wire, so there is no per-language server component" — Solari
  *invites* community SDKs, and .NET is the biggest ecosystem without one.
- **Anyone can use it.** Drop `Solari.Sdk` into any ASP.NET Core project,
  register it with `AddSolari(...)`, and you're calling Solari from C# with
  full IntelliSense, typed models, and DI-friendly `HttpClient` pooling.
- **It ships with a working example**, not just wrapper code: a small
  ASP.NET Core API (`samples/Solari.Sdk.Sample`) that runs untrusted code
  snippets inside disposable Solari sandboxes — a pattern real products use
  (coding-interview graders, "try it online" boxes, tool-calling backends for
  agents) instead of running arbitrary code on their own servers.

## What's in the repo

```
src/Solari.Sdk/            The SDK itself (net8.0 class library, zero NuGet deps)
  SolariClient.cs             Entry point: client.Sessions / .Sandboxes / .Desktops
  SessionsClient.cs            Cloud browsers  -> POST/DELETE /sessions
  SandboxesClient.cs           Sandboxes       -> /sandboxes, /exec, file upload/download
  DesktopsClient.cs            Desktop VMs     -> /desktops, pause/resume
  SolariApiException.cs        Typed errors (400/401/402/404/429/502/503) with .IsRetryable
  Extensions/                  AddSolari(...) for ASP.NET Core DI
  Models/                      Typed request/response records for every endpoint

samples/Solari.Sdk.Sample/  ASP.NET Core minimal API: POST /api/run executes a
                             code snippet inside a fresh Solari sandbox and
                             tears it down afterwards, every time.

tests/Solari.Sdk.Tests/     7 tests against a fake HttpMessageHandler, verifying
                             auth headers, request bodies, URL-encoding of
                             sandbox/session IDs, and error/retry semantics.
```

## Quickstart

```bash
export SOLARI_API_KEY=slr_live_...   # from https://console.getsolari.com
                                      # (promo code STARTER1MO-MKY4BNDK = 1 month free)
```

```csharp
using Solari.Sdk;
using Solari.Sdk.Models;

var client = new SolariClient(new SolariOptions { ApiKey = apiKey });

// Spin up a sandbox, run a command, tear it down.
var sandbox = await client.Sandboxes.CreateAsync(new CreateSandboxRequest(Cpu: 1, MemMb: 512));
var result  = await client.Sandboxes.ExecAsync(sandbox.SandboxId, new ExecRequest("sh", new[] { "-c", "echo hello" }));
Console.WriteLine(result.Stdout); // "hello\n"
await client.Sandboxes.DeleteAsync(sandbox.SandboxId);

// Launch a cloud browser and get a CDP endpoint for Playwright/Puppeteer.
var session = await client.Sessions.CreateAsync(new CreateSessionRequest(Stealth: true));
// session.CdpEndpoint -> chromium.connectOverCDP(session.CdpEndpoint)
await client.Sessions.ReleaseAsync(session.SessionId);
```

In ASP.NET Core, register it with DI instead of `new`-ing it up, so the
underlying `HttpClient` is pooled by `IHttpClientFactory`:

```csharp
builder.Services.AddSolari(options =>
{
    options.ApiKey = builder.Configuration["Solari:ApiKey"]!;
});

app.MapPost("/run", async (SolariClient solari, ...) => { ... });
```

## Running the sample

```bash
export SOLARI_API_KEY=slr_live_...
dotnet run --project samples/Solari.Sdk.Sample
curl -X POST http://localhost:5000/api/run \
  -H "Content-Type: application/json" \
  -d '{"language":"python","code":"print(2 + 2)"}'
# {"exitCode":0,"stdout":"4\n","stderr":""}
```

## Running the tests

```bash
dotnet run --project tests/Solari.Sdk.Tests
```

**A note on the test project's shape:** this repo was built in a sandboxed
environment whose network policy blocks `nuget.org`, so `xunit` couldn't be
restored there. Rather than skip tests, `tests/Solari.Sdk.Tests` is a plain
console app with a ~15-line reflection-based runner and a hand-rolled
`Assert` class — it exercises the exact same scenarios an xunit `[Fact]`
suite would (auth headers, request bodies, ID URL-encoding, retryable vs.
non-retryable errors) against a fake `HttpMessageHandler`, with zero
external dependencies. On a machine with normal NuGet access, swapping this
project back to `dotnet new xunit` is a drop-in change — the test bodies in
`SolariClientTests.cs` translate 1:1 to `[Fact]` methods.

The `Solari.Sdk` library itself has **zero external NuGet dependencies** —
it pulls `Microsoft.Extensions.Http`/`DependencyInjection.Abstractions` from
the ASP.NET Core shared framework via `<FrameworkReference>` rather than a
package reference, so it restores and builds anywhere the .NET SDK is
installed, no network required.

## API coverage

| Resource   | Endpoints implemented                                                        |
|------------|-------------------------------------------------------------------------------|
| Sessions (browsers) | `POST /sessions`, `DELETE /sessions/{id}`                            |
| Sandboxes  | `POST /sandboxes`, `POST /sandboxes/{id}/exec`, upload/download-url minting + convenience upload/download, `DELETE /sandboxes/{id}` |
| Desktops   | `POST /desktops`, `GET /desktops/{id}`, `POST /desktops/{id}/pause`, `POST /desktops/{id}/resume`, `DELETE /desktops/{id}` |

Not yet implemented (good first-issue territory): the sandbox/desktop
`control` WebSocket (for interactive commands and computer-use actions),
session profiles/proxies/regions passthrough, and desktop snapshots/templates.

## License

MIT
