---
name: dotnet-performance.instructions
description:
    Use when reviewing, improving or designing C# and .NET performance, allocation behavior, async flow, collection
    usage, or hot-path code without weakening correctness.
applyTo: "**/*.cs, **/*.md"
---

# .NET performance guidance

## Principles

- Measure before optimizing. Prefer evidence from benchmarks, profiling, allocations, traces, or repository performance
  tests over intuition.
- Preserve correctness, contracts, and readability unless the performance constraint is real and material.

    # .NET performance guidance

    ## Principles
    - Measure before optimizing. Prefer evidence from benchmarks, profiling, allocations, traces, or repository
      performance tests over intuition.
    - Preserve correctness, contracts, and readability unless the performance constraint is real and material.
    - Optimize the actual hot path, not the code that merely looks expensive.
    - Treat allocations as a first-class concern in hot paths.
        - Avoid avoidable closures, iterator allocations, temporary collections, or repeated string concatenation when
          measurements show they matter.
        - Reuse buffers, builders, or cached delegates only when the lifetime and thread-safety model stay clear.
    - Keep async flows fully async for I/O-bound work. Avoid sync-over-async and unnecessary task wrapping.
    - Use simpler algorithms or data structures before reaching for micro-optimizations.
    - Prefer returning existing instances when the observable state did not change and the contract allows it.
    - Be cautious with LINQ in hot paths. Keep it when clarity wins and the cost is negligible; replace it when
      profiling shows real overhead.
    - When using spans, pooling, or lower-level optimizations, keep the code obviously safe and document the constraint
      being optimized.
    - Validate performance-sensitive changes with the narrowest relevant benchmark, test, or runtime check available.

    ## Modern C# / .NET (net10+) Performance

    ### Measurement & Diagnostics
    - Measure before optimizing: use `BenchmarkDotNet` for microbenchmarks and representative workload tests for
      end-to-end regression checks.
    - Use the .NET diagnostics toolset for investigation: `dotnet-trace`, `dotnet-counters`, `dotnet-dump`,
      `dotnet-symbol`, `dotnet-monitor`, and platform tools (Linux `perf`, PerfView on Windows).

    ### Hot-path & Memory
    - Treat allocations as primary signal in hot paths: prefer `Span<T>`/`Memory<T>`, `stackalloc`, `ArrayPool<T>`, and
      reusable buffers; avoid ephemeral allocations in tight loops.
    - Profile live workloads safely: prefer low-overhead counters/metrics then escalate to sampling traces. Use
      diagnostic ports to capture startup traces when necessary.
    - Async and concurrency: keep async flows end-to-end; avoid `async void`; consider `ValueTask` on highly-invoked
      async methods only after measurement.
    - ThreadPool and blocking: avoid blocking thread-pool threads; favor `Task.Run` for CPU-bound work and configure
      `ThreadPool` min threads only when measured beneficial.
    - Garbage Collector (GC): prefer server/background GC for server workloads; avoid frequent LOH/POH allocations; use
      pooling and buffer reuse to reduce GC pressure; tune via `runtimeconfig.template.json` or environment variables
      when justified.

    ### Runtime, JIT & CI
    - JIT/AOT and startup: measure tradeoffs between tiered compilation, ReadyToRun, and NativeAOT; use profile-guided
      JIT or AOT only when it reduces startup or critical-path latency and has been validated.
    - Serialization and I/O: prefer `System.Text.Json` with source generators for hot-path serialization; avoid
      reflection-based serializers in performance-critical code.
    - Inlining and hot-path attributes: rely on the JIT's heuristics; use aggressive inlining or `MethodImplOptions`
      sparingly and only when benchmarks show benefit.
    - Observability vs overhead: instrument with structured logs (`ILogger`), metrics
      (System.Diagnostics.Metrics/OpenTelemetry), and traces (OpenTelemetry). Configure sampling and cardinality limits
      for production.
    - Runtime configuration: use `runtimeconfig.template.json`, MSBuild properties, or environment variables to set
      GC/threadpool/jit knobs; remember environment variables take precedence since .NET 9.
    - CI and regression testing: include performance checks in CI where practical (separate benchmark jobs), and gate
      changes that introduce measurable regressions for documented workloads.
    - Documentation and constraints: for any optimization, document the performance constraint, benchmark harness, and
      acceptance criteria so future maintainers can validate regressions.
