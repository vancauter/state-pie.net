---
name: llama-sharp.chat-session
description: Patterns for implementing chat sessions and interactive workflows.
---

# Purpose

Describe practical patterns for building and operating chat sessions: lifecycle, history management, restart/rewind, and
snapshot/restore behavior.

# When to use

- Building multi-turn chat flows with either stateful or stateless execution modes.
- Implementing session persistence, restart, or rewind semantics.

# Key concepts

- Session types: stateful (preserves KV cache and history) vs stateless (rebuilds context per turn).
- History management: truncation, summarization, external memory (RAG).
- Restart & rewind: safe reset and stepping back to previous states.
- Role handling and prompt separators: consistent system/user/assistant ordering.
- Token accounting and budget hooks.

# Guidance

- Use stateless mode for short or low-resource queries; use stateful mode for extended multi-turn dialogs.
- Expose import/export snapshot APIs for persistence and replication.

# Example

# Example

```csharp
var session = new ChatSession(executor);
session.Append("user", "Hello");
var reply = session.Step();
var snapshot = session.SaveState();
var restored = ChatSession.Restore(snapshot);
```

# Integration notes

- Provide both human-readable and compact binary snapshot formats depending on use case.
- Ensure session close semantics release GPU resources deterministically.

# References

- ChatSession tutorial: https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/ChatSession/
- ChatSession examples: https://scisharp.github.io/LLamaSharp/0.25.0/Examples/ChatSessionWithHistory/
- Examples: https://scisharp.github.io/LLamaSharp/0.25.0/Examples/LoadAndSaveSession/
