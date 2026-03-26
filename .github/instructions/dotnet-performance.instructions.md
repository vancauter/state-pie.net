---
name: "dotnet-performance"
description:
    "Use when reviewing or improving C# and .NET performance, allocation behavior, async flow, collection usage, or
    hot-path code without weakening correctness."
---

# .NET performance guidance

- Measure before optimizing. Prefer evidence from benchmarks, profiling, allocations, traces, or repository performance
  tests over intuition.
- Preserve correctness, contracts, and readability unless the performance constraint is real and material.
- Optimize the actual hot path, not the code that merely looks expensive.
- Treat allocations as a first-class concern in hot paths.
    - Avoid avoidable closures, iterator allocations, temporary collections, or repeated string concatenation when
      measurements show they matter.
    - Reuse buffers, builders, or cached delegates only when the lifetime and thread-safety model stay clear.
- Keep async flows fully async for I/O-bound work. Avoid sync-over-async and unnecessary task wrapping.
- Use simpler algorithms or data structures before reaching for micro-optimizations.
- Prefer returning existing instances when the observable state did not change and the contract allows it.
- Be cautious with LINQ in hot paths. Keep it when clarity wins and the cost is negligible; replace it when profiling
  shows real overhead.
- When using spans, pooling, or lower-level optimizations, keep the code obviously safe and document the constraint
  being optimized.
- Validate performance-sensitive changes with the narrowest relevant benchmark, test, or runtime check available.
