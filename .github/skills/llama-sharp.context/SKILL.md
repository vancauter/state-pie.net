---
name: llama-sharp.context
description: Context parameters and best practices for LlamaSharp executors and sessions.
---

# Purpose

Explain how to structure and manage model-level context parameters (`IContextParams`) and application-level prompt
windowing so integrators avoid common pitfalls (silent truncation, RoPE/YARN mismatch, OOMs).

# When to use

- Integrating a new GGUF/quantized model and validating `n_ctx` and tokenizer behavior.
- Implementing prompt trimming, summarization, or retrieval-augmented insertion (RAG).

# Key concepts

- Token counting: prefer the runtime tokenizer over naive character heuristics.
- Common `IContextParams`: `ContextSize`/`n_ctx`, `BatchSize`/`n_batch`, `EmbeddingMode`, RoPE/Yarn parameters,
  `TypeK`/`TypeV`, and thread-related settings.
- Sliding-window strategies: drop old turns, summarize, or insert retrieved context items.
- Prompt composition: canonical order is system -> user -> assistant; maintain consistent separators and role labels.

# Guidance

- Compute token lengths with `PromptBuilder.TokenCount(text)` (runtime tokenizer preferred) and surface counts in logs.
- Prefer application-level trimming/summarization rather than altering low-level `IContextParams` unless well-tested.
- Use `EmbeddingMode` for embedding-only workloads to avoid decode passes.
- When tuning throughput, co-tune `BatchSize` with executor batch strategies and monitor memory metrics.

# Example

```csharp
var prompt = PromptBuilder.Build(history, systemPrompt);
var tokens = PromptBuilder.TokenCount(prompt);
if (tokens > maxTokens) prompt = PromptBuilder.TrimToWindow(prompt, maxTokens);
var result = executor.Run(promptChunk); // may be BatchedExecutor or StatelessExecutor
```

## Notes

- Changing LLamaContext internals (RoPE/YARN/TypeK/TypeV) can affect generation semantics; test and provide rollback.
- Persist KV-cache snapshots (`SessionSnapshot.Save()` / `.Load()`) for fast resume in stateful ChatSessions.
- Align prompt tokenization and context assumptions with the GGUF model's `n_ctx`.

## Suggested utilities for this repo

- `PromptBuilder.Build(history, systemPrompt)` — compose prompts.
- `PromptBuilder.TokenCount(text)` — wrapper for the runtime tokenizer.
- `PromptBuilder.TrimToWindow(prompt, maxTokens)` — deterministic trimming preserving system instructions.
- `SessionSnapshot.Save()` / `.Load()` — persist KV-cache + token offsets.

## Quick checklist when integrating a model

- Confirm GGUF/quantized model `n_ctx` matches app assumptions.
- Validate token counting matches runtime tokenizer.
- Run representative generation tests after changes to RoPE/Yarn/TypeK/TypeV.

# References

- Understand LLamaContext tutorial: https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/UnderstandLLamaContext/
- IContextParams API docs: https://scisharp.github.io/LLamaSharp/0.25.0/xmldocs/llama.abstractions.icontextparams/
- LLamaSharp overview: https://scisharp.github.io/LLamaSharp/0.25.0/
