# RequestPlayback (Modernization Plan)

## 1. Executive Summary
RequestPlayback (currently pmilet/playback) is an ASP.NET Core middleware and supporting services that record and replay HTTP requests (inbound) and HTTP client calls (outbound). This plan modernizes the codebase to align with .NET 8+ best practices, improve developer ergonomics, add privacy controls, enhance observability, enable regression (golden master) testing, and modularize optional functionality while keeping the library lightweight.

## 2. Core Goals
1. Target latest LTS (.NET 8) and adopt nullable reference types.
2. Simplify architecture: clear separation of recording, replay, storage, transforms.
3. Modular packaging: core stays small; optional features split.
4. Production-safe privacy (redaction, encryption, schema versioning).
5. Observability by default (OpenTelemetry, structured logging).
6. Regression validation (semantic diff engine).
7. Modern fault/latency injection via HttpClient DelegatingHandlers (replace custom factory).
8. Async, streaming, cancellation-aware IO for performance.
9. Minimal setup: 5–10 lines to enable in a new project.
10. Backwards compatibility via adapters and staged deprecation.

## 3. Current State (Inferred)
- Single project mixing middleware, storage services, Swagger filter, error simulation.
- Custom HttpClientFactory wrapper.
- Storage implemented with inheritance (PlaybackStorageServiceBase).
- EventSource-based logging; limited modern tracing/metrics.
- No explicit schema/versioning or redaction pipeline.
- Replay triggered by headers (X-Playback-Mode, X-Playback-Id) but lacks formal diff validation tools.

## 4. Target Architecture

```
+-----------------------------------------------------------+
| RequestPlayback.AspNetCore (Core)                        |
|  - Middleware (PlaybackMiddleware)                       |
|  - Recorder (IPlaybackRecorder)                          |
|  - Replayer (IPlaybackReplayer)                          |
|  - Storage Abstractions (IPlaybackStorage)               |
|  - Options (PlaybackOptions)                             |
|  - Basic File Storage                                    |
|  - ActivitySource + ILogger integration                  |
+-----------------------------------------------------------+
          |               |                 |
          v               v                 v
   +-------------+   +--------------+   +------------------+
   | Transforms  |   | FaultInjection|  | Testing (Diff)   |
   | (Redaction, |   | Delegating    |  | Golden Master    |
   | Normalization)| | Handlers)     |  | Validation APIs  |
   +-------------+   +--------------+   +------------------+
          |
          v
   +------------------+
   | Storage Providers|
   | (Azure Blob, S3, |
   | LiteDB Index)    |
   +------------------+

Cross-cutting: OpenTelemetry (traces/metrics), Encryption, Compression, Admin Minimal APIs.
```

## 5. Modular Package Layout

| Package | Description |
|---------|-------------|
| RequestPlayback.AspNetCore | Core middleware, file storage, basic replay/record |
| RequestPlayback.Storage.AzureBlob | Azure Blob storage implementation |
| RequestPlayback.Storage.Abstractions | Shared interfaces (if split desired) |
| RequestPlayback.FaultInjection | DelegatingHandlers for latency/errors |
| RequestPlayback.Swagger | Swagger/OpenAPI enrichment |
| RequestPlayback.Testing | Diff engine & golden master helpers |
| RequestPlayback.Encryption (future) | AES-GCM and key management integration |
| RequestPlayback.Tools (future) | CLI utilities (list, diff, prune) |

## 6. Phased Roadmap

### Phase 1 – Foundation (Modern .NET)
- Migrate to net8.0; enable `<Nullable>enable</Nullable>`.
- Introduce PlaybackOptions (strongly typed).
- Define interfaces: IPlaybackStorage, IPlaybackRecorder, IPlaybackReplayer, IPlaybackTransform.
- Refactor middleware to use these interfaces (composition over inheritance).
- Replace custom HttpClientFactory with IHttpClientFactory + DelegatingHandler for fault injection.
- Provide FileSystemPlaybackStorageService (async streaming).
- Add ActivitySource ("RequestPlayback") and structured ILogger usage.
- Backward compatibility adapters for existing classes; mark legacy with `[Obsolete]`.

### Phase 2 – Privacy & Schema
- PlaybackEnvelope (record) with SchemaVersion.
- JSON serialization using System.Text.Json with deterministic ordering.
- Redaction pipeline (headers + JSON path rules).
- Optional encryption (interface IPlaybackEncryptor).
- Compression option (Gzip first; Zstd later).
- Add integrity hash and store metadata for replay validation.

### Phase 3 – Regression / Diff
- Validation mode: record expected vs current response (semantic diff).
- Diff engine tolerances (ignore timestamps, GUID patterns, ordering for arrays optionally).
- Test helpers: `[PlaybackData("id1","id2")]` attribute feeding xUnit/ NUnit.
- CLI (optional) or MSBuild target to auto-verify stored envelopes.
- Diff artifact (JSON) with classification: added, removed, changed fields.

### Phase 4 – Observability & Admin
- Metrics (Prometheus/OpenTelemetry): playback_record_total, playback_replay_hit_total, playback_replay_miss_total, playback_latency_ms histogram.
- Minimal APIs:
  - GET /playback/{id}
  - GET /playback/search?method=&path=&since=
  - POST /playback/{id}/validate
  - DELETE /playback/{id}
  - GET /playback/stats
- Optional SQLite/LiteDB index for efficient search.
- Dash-friendly JSON responses.

### Phase 5 – Advanced Fault Injection
- Profiles (latency distributions: fixed, gaussian, percentile script).
- Config via PlaybackOptions.FaultProfiles.
- Exception matrices (per host/path).
- Bandwidth throttling (stream wrapper that drip-feeds response).
- Scenario-level fault injection tied to playback id or tag.

### Phase 6 – Developer Experience & Docs
- New README with “5-minute QuickStart”.
- Samples:
  - Minimal API (Program.cs only).
  - MVC + Swagger + Replay sample.
  - Golden master regression sample.
  - Fault injection resilience sample (Polly integration).
- Migration guide (v1 → v2) enumerating renames and changes.
- Contributing guidelines.

## 7. Detailed Tasks

### 7.1 Interfaces
```csharp
public interface IPlaybackStorage {
    Task<bool> ExistsAsync(PlaybackId id, CancellationToken ct);
    Task<PlaybackEnvelope?> ReadAsync(PlaybackId id, CancellationToken ct);
    Task WriteAsync(PlaybackEnvelope envelope, CancellationToken ct);
    IAsyncEnumerable<PlaybackMetadata> QueryAsync(PlaybackQuery query, CancellationToken ct);
}

public interface IPlaybackRecorder {
    Task<PendingRecording> CaptureRequestAsync(HttpContext context, CancellationToken ct);
    Task<PlaybackEnvelope> FinalizeAsync(PendingRecording pending, HttpContext context, CancellationToken ct);
}

public interface IPlaybackReplayer {
    Task<ReplayResult> TryReplayAsync(HttpContext context, CancellationToken ct);
}

public interface IPlaybackTransform {
    ValueTask<PlaybackEnvelope> ApplyAsync(PlaybackEnvelope envelope, CancellationToken ct);
}
```

### 7.2 Envelope Model
```csharp
public enum PlaybackSchemaVersion { V1 = 1, V2 = 2 }

public sealed record PlaybackEnvelope(
    PlaybackId Id,
    DateTimeOffset RecordedAt,
    string HttpMethod,
    string Path,
    IReadOnlyDictionary<string,string> RequestHeaders,
    byte[] RequestBody,
    int ResponseStatusCode,
    IReadOnlyDictionary<string,string> ResponseHeaders,
    byte[] ResponseBody,
    PlaybackSchemaVersion SchemaVersion,
    string? ContentType = null,
    string? Tag = null);
```

### 7.3 Middleware (Simplified)
```csharp
public sealed class PlaybackMiddleware {
    private readonly RequestDelegate _next;
    public PlaybackMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context,
                                  IPlaybackReplayer replayer,
                                  IPlaybackRecorder recorder,
                                  IPlaybackStorage storage,
                                  ILogger<PlaybackMiddleware> logger,
                                  ActivitySource source,
                                  CancellationToken ct = default) {
        using var activity = source.StartActivity("Playback");
        var mode = PlaybackModeDecider.GetMode(context); // static helper or injected service
        activity?.SetTag("playback.mode", mode.ToString());

        if (mode == PlaybackMode.Replay) {
            var result = await replayer.TryReplayAsync(context, ct);
            if (result.IsHit) {
                activity?.SetTag("playback.hit", true);
                logger.LogInformation("Replayed {PlaybackId}", result.PlaybackId);
                return;
            }
            activity?.SetTag("playback.hit", false);
        }

        if (mode == PlaybackMode.Record) {
            var pending = await recorder.CaptureRequestAsync(context, ct);
            await _next(context);
            var envelope = await recorder.FinalizeAsync(pending, context, ct);
            await storage.WriteAsync(envelope, ct);
            logger.LogInformation("Recorded {PlaybackId}", envelope.Id.Value);
            return;
        }

        await _next(context);
    }
}
```

### 7.4 Fault Injection DelegatingHandler
```csharp
public sealed class PlaybackFaultInjectionHandler : DelegatingHandler {
    private readonly IFaultProfileProvider _profiles;
    private readonly Random _rng = Random.Shared;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) {
        var profile = _profiles.GetFor(request);
        if (profile is null) return await base.SendAsync(request, ct);

        if (profile.Latency is { } latency) {
            var delay = Gaussian(latency.MeanMs, latency.StdDevMs);
            await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(0, delay)), ct);
        }
        if (profile.ExceptionProbability > 0 && _rng.NextDouble() < profile.ExceptionProbability) {
            throw new PlaybackFakeException("Simulated fault");
        }
        return await base.SendAsync(request, ct);
    }
}
```

### 7.5 Semantic Diff Outline
Diff categories:
- MissingFields
- AdditionalFields
- ChangedValues (with old/new)
- Ignored (matches ignore rules)
Rules config:
```json
{
  "ignorePaths": ["$.timestamp", "$.meta.requestId"],
  "numericTolerance": 0.0001,
  "unorderedArrays": ["$.items"]
}
```

### 7.6 Metrics (names)
- playback_record_total (counter)
- playback_replay_hit_total (counter)
- playback_replay_miss_total (counter)
- playback_record_bytes (histogram)
- playback_replay_latency_ms (histogram)

### 7.7 Privacy Transform Examples
- Header redaction: Authorization, Cookie
- JSON path masking: replace emails with hash
- Deterministic GUID normalization (map encountered GUIDs to stable placeholders GUID_1, GUID_2)

### 7.8 Admin Minimal APIs
```csharp
app.MapGroup("/playback")
   .MapGet("/{id}", PlaybackHandlers.GetEnvelope)
   .MapGet("/search", PlaybackHandlers.Search)
   .MapPost("/{id}/validate", PlaybackHandlers.Validate)
   .MapDelete("/{id}", PlaybackHandlers.Delete)
   .MapGet("/stats", PlaybackHandlers.Stats);
```

## 8. Acceptance Criteria

| Phase | Criteria |
|-------|----------|
| 1 | Builds on net8.0, nullable enabled, new interfaces in place, adapter layer works |
| 2 | Redaction rules configurable, envelope includes schema version, encrypted storage option documented |
| 3 | Diff engine returns structured JSON; sample regression test passes in CI |
| 4 | Metrics exposed; admin endpoints functional; search returns expected results |
| 5 | Fault profiles configurable; latency & exception injection observable in traces |
| 6 | Updated README + samples; migration guide published |

## 9. Migration Strategy
1. Introduce new namespaces side-by-side (no breaking changes).
2. Mark legacy classes `[Obsolete("Use RequestPlayback.*")]` after stable new API release.
3. Provide conversion script or instructions for Startup to Program.cs minimal hosting model.
4. Version bump to 2.0.0 when removing deprecated code.

## 10. Risk & Mitigation

| Risk | Mitigation |
|------|------------|
| Breaking existing users | Adapters + dual registration period |
| Performance regressions with new transforms | Benchmark harness (BenchmarkDotNet) before release |
| Privacy misconfiguration | Safe defaults (denylist headers), docs & warnings |
| Complexity creep | Keep core < 15 public types; move extras into optional packages |
| Storage inconsistency | Add integrity hash + validation routine |

## 11. Future Enhancements (Post-plan)
- CLI: list, diff, prune, export/import playback sets.
- UI dashboard (Blazor) for browsing and diffing.
- Scenario sequences (multi-request replay).
- Parameterized playback (variable substitution).
- gRPC support.
- Source generators for strongly-typed playback IDs and diff ignore attribute.

## 12. QuickStart (Target State)
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRequestPlayback(p => p
    .RecordIncoming()
    .UseFileStorage(Path.Combine(builder.Environment.ContentRootPath,"playbacks"))
    .RedactHeaders("Authorization","Cookie"));

var app = builder.Build();
app.UseRequestPlayback();
app.MapPlaybackAdmin(); // optional
app.MapGet("/hello", () => "world");
app.Run();
```

## 13. Naming & Packaging
- NuGet: RequestPlayback.AspNetCore
- Namespaces: RequestPlayback.*, minimal cross-cutting dependencies
- Optional packages follow same prefix.

## 14. Tooling & CI
- GitHub Actions: build, test, publish coverage, run diff validation sample.
- Code analyzers: .editorconfig + Roslyn rules (CA, ID, formatting).
- Security scanning (Dependabot).
- Performance benchmark workflow on PR (opt-in label).

## 15. Documentation Outline
- README (Concept + QuickStart)
- PRIVACY.md (redaction/encryption guidelines)
- DIFF_TESTING.md (golden master usage)
- FAULT_INJECTION.md (profiles configuration)
- MIGRATION_V1_TO_V2.md (step-by-step)

## 16. Open Questions
1. Do we keep Swagger integration in core or move to package?
2. Which encryption mechanism (DataProtection vs custom key provider)?
3. Minimum index solution: JSON manifest vs LiteDB vs SQLite?
4. Should playback ids become opaque GUIDs with metadata separate (improves privacy)?
5. Need for built-in request verification counts (like mock frameworks) or leave to testing package?

## 17. Next Immediate Actions
1. Create branch: feature/modernization-phase1
2. Add net8.0 target + nullable, introduce new interfaces.
3. Refactor middleware to new composition model.
4. Provide adapter around existing PlaybackStorageServiceBase.
5. Update README with "Modernization In Progress" notice.

---

End of MODERNIZATION_PLAN.md