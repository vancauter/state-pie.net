---
name: llama-sharp-architecture
description: Essentials about LlamaSharp architecture for implementers and integrators.
---

# Purpose

Provide a concise, practical overview of the LlamaSharp runtime components and how they fit into an application
architecture.

# When to use

- Planning integration points between model loading, executors, sessions, and persistence.
- Designing resource and lifetime management for CPU/GPU-backed inference.

# Key concepts

- Model core: model artifact, tokenizer, and runtime representation.
- Executors: single, batched, and guidance-aware executors that manage KV cache and inference loops.
- Runtimes: CPU/GPU backends, memory constraints, and plugin/extension hooks.
- Sessions: stateful vs stateless sessions holding history and caches.
- State management: load/save model & context state, rewind and resume, snapshotting patterns.
- Thread-safety and batching: coordinate concurrency, batching, and OOM mitigation.

# Guidance

- Initialize the model once and reuse executors where appropriate to amortize load time.
- Prefer small, inspectable session state and serialize snapshots for persistence and transfer.

# Example

```csharp
var model = Model.Load(path);
var executor = new BatchedExecutor(model);
var session = new ChatSession(executor);
var response = session.Execute("Hello");
```

# Integration notes

- Manage GPU lifetimes explicitly to avoid resource leakage.
- Design executors to accept guidance callbacks rather than embedding hard-coded control flow, making them composable
  and testable.

# References

- Architecture overview: https://scisharp.github.io/LLamaSharp/0.25.0/Architecture/
- Use executors: https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/Executors/
- LLamaSharp overview: https://scisharp.github.io/LLamaSharp/0.25.0/
