---
name: dotnet-architecture.instructions
description:
    Use when working on C# or .NET project files or documentation and you need architecture, design, and implementation
    guidance proportionate to the codebase.
applyTo: "**/*.cs, **/*.csproj, **/*.sln, **/*.md"
---

# Architecture Guidance

## General Principles

- Match the architecture to the size and complexity of the codebase.
    - Small libraries or services should stay simple and avoid ceremonial layers.
    - Medium solutions should separate responsibilities when that clarifies dependencies, ownership, or deployment
      boundaries.
    - Larger systems should make module boundaries, contracts, and ownership explicit.
- Keep domain language consistent across types, namespaces, tests, and documentation.
- Prefer changes that solve the root cause without expanding the abstraction surface unnecessarily.
- Preserve public contracts unless the task explicitly calls for a breaking change.
- Prefer composition over inheritance unless substitution is a real part of the design.
- Use modern C# and .NET features when they improve clarity, correctness, or safety, not just because they are
  available.
- Keep async code end to end for I/O-bound work. Avoid blocking on tasks or mixing sync and async flows without a clear
  reason.
- Validate inputs at the boundary where invalid data enters the system. Keep business rules near the behavior they
  protect.
- When editing project files, keep dependencies deliberate, package additions justified, and target-framework changes
  explicit.
- Add or update tests when behavior changes, and run the narrowest relevant validation before finishing.

## Modern C# / .NET (net10+)

- Target frameworks deliberately: prefer `net10.0` for new projects and multi-target only when necessary for broad
  compatibility.
- Use the Generic Host (`IHost`/`HostBuilder`) and the built-in DI container as the composition root; keep registration
  and startup wiring in `Program.cs` or a single composition module.
- Prefer minimal APIs for small services and lightweight endpoints; use Controllers and explicit layering for larger,
  feature-rich applications.
- Keep async flows end-to-end: prefer `Task`/`ValueTask`, `IAsyncEnumerable<T>` for streams, and always accept
  `CancellationToken` on async APIs.
- Embrace modern C# language features that improve correctness and clarity: nullable reference types, `record` types for
  immutable DTOs, pattern matching, `init` setters, file-scoped namespaces, and `global using` where it reduces noise.
- Use low-allocation patterns on hot paths: `Span<T>`/`Memory<T>`, `ArrayPool<T>`, and consider `ValueTask` only when
  measured beneficial. Prefer clarity over micro-optimizations unless profiling shows hotspots.
- Leverage `System.Text.Json` and its source generators for high-performance serialization; prefer configuration-light
  serializers and avoid runtime reflection costs on hot paths.
- Use source generators for compile-time code generation (mapping, diagnostics, serialization) to reduce runtime
  overhead and improve correctness.
- Design for observability from day one: structured logging via `ILogger`, distributed tracing and metrics using
  OpenTelemetry, and expose health checks and metrics endpoints for platform ingestion.
- Prefer a modular-monolith approach for internal boundaries: small, well-defined projects/libraries with clear
  interfaces; avoid microservices until teams and operational practices justify the complexity.
- Use `IHostedService`/`BackgroundService` for background work and ensure graceful shutdown semantics are implemented
  and tested.
- Follow cloud-native and 12-factor principles: configuration from environment or providers, externalize state, design
  for immutability of containers, and store secrets in dedicated secret stores.
- Use gRPC for high-throughput, low-latency internal RPC; use HTTP/3 and modern protocols where supported by the
  platform and clients.
- Keep dependencies minimal and explicit: prefer stable, well-maintained packages, run dependency audits, and pin or
  review transitive upgrades in PRs.
- Enforce security by default: secure Kestrel/transport channels, validate inputs at boundaries, use established
  authz/authn middleware, and avoid embedding secrets in source.
- Automate testing and validation: unit tests, integration tests (use lightweight in-memory or testcontainers where
  appropriate), contract tests for service boundaries, and performance tests for critical paths.
- Use tooling and CI: enable Roslyn analyzers, `dotnet format`, nullable warnings as errors where practical, and run
  `dotnet test` and `dotnet build` in CI gating changes.
- Measure before optimizing: use BenchmarkDotNet, `dotnet-trace`, `dotnet-counters`, and profiling tools; document the
  constraint and test case when applying optimizations.
- Prefer explicit cancellation/timeouts for outbound calls and put resilience (retries, circuit-breakers) in
  infrastructure layers rather than business logic.
- Encourage consistent code style for modern constructs (top-level programs, implicit usings) but only adopt team-wide;
  do not mix patterns inside the same project.
