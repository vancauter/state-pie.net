---
name: llama-sharp-executors
description: Guidance for implementing and using executors in LlamaSharp-style runtimes.
---

# Purpose

Explain executor responsibilities, batching strategies, guidance hooks, and error-handling patterns for LlamaSharp-style
runtimes.

# When to use

- Implementing or extending runtime executors (Interactive, Instruct, Batched, Stateless) and their orchestration.

# Key concepts

- Executor role: consume tokenized inputs, maintain KV cache, and produce logits/tokens.
- Types: single-step, batched, guided (control tokens/weights), and rewind-capable executors.
- Batching: align prompt positions, pad inputs, and merge states safely.
- Guidance hooks: user-supplied callbacks to alter logits, stop conditions, or scoring.
- Error handling: detect OOM and fall back to smaller batches or alternate runtimes.

# Guidance

- Keep executors stateless between top-level calls except for managed caches and explicit snapshots.
- Provide clear APIs for advancing tokens and rewinding to prior positions.

# Example

```csharp
var batch = new[] { sessionA.NextChunk(), sessionB.NextChunk() };
var results = executor.Run(batch);
var resultsWithGuidance = executor.Run(batch, guidanceCallback);
```

# Integration notes

- Validate alignment of KV cache sizes across batched inputs.
- Expose throughput and latency metrics to tune batch sizing.

# References

- Use executors tutorial: https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/Executors/
- Batched executor examples: https://scisharp.github.io/LLamaSharp/0.25.0/Examples/BatchedExecutorGuidance/
- Executor save/load examples: https://scisharp.github.io/LLamaSharp/0.25.0/Examples/LoadAndSaveState/
