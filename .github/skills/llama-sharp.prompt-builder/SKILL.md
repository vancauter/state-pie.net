---
name: llama-sharp.prompt-builder
description: Patterns and examples for composing and trimming prompts for LLamaSharp ChatSession and executors.
---

# Purpose

Provide a concise, actionable reference for building a `PromptBuilder` utility: composing role-aware prompts, estimating
token counts, and trimming history to fit a model `n_ctx` window.

# When to use

- Implementing prompt composition and trimming logic for ChatSession-driven or executor-driven flows.

# Key concepts

- Use `IHistoryTransform` to convert `ChatHistory` <-> text.
- Implement `ITextTransform` / `ITextStreamTransform` for input/output processing in `ChatSession`.
- Prefer runtime tokenizer counts (via `LLamaContext` or ModelManager) over character heuristics.

# Guidance

- Minimal recommended API
    - `string BuildPrompt(ChatHistory history, string userInput, string? systemPrompt = null)`
    - `int EstimateTokenCount(string text)`
    - `string TrimToWindow(string prompt, int maxTokens)`

- Conservative token-count heuristic (temporary fallback)

```csharp
int EstimateTokens(string s) => Math.Max(1, s.Length / 4);
```

- Trim loop (pattern)

1. available = n_ctx - reservedForResponse - safetyMargin
2. while (EstimateTokens(prompt) > available) remove earliest history item

- Integration notes

- Provide `PromptBuilder` as a small, side-effect-free utility and keep trimming logic deterministic and unit-testable.
- Expose both `BuildPrompt(history, input)` (returns string) and `EstimateTokenCount(string)` so callers can instrument
  and tune windows.
- Replace conservative heuristics with the runtime tokenizer when a tokenizer API is available.

# Example

```csharp
var text = historyTransform.HistoryToText(history);
var prompt = system + "\n" + text + "\nUser: " + userInput;
while (EstimateTokens(prompt) > availableTokens) {
      history.RemoveOldest();
      prompt = system + "\n" + historyTransform.HistoryToText(history) + "\nUser: " + userInput;
}
```

# References

- ChatSession docs: https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/ChatSession/
- Context and n_ctx: https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/UnderstandLLamaContext/
- Executors & inference params: https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/Executors/
