---
name: llama-sharp-snapshot
description: Save/load patterns and safe snapshot handling for LLamaSharp ChatSession and executor state.
---

# Purpose

Document recommended patterns for persisting and restoring ChatSession and executor/context state, and provide practical
guidance (metadata, validation, store abstraction).

# When to use

- Persisting user sessions for resume, crash recovery, or migration across hosts.
- Implementing snapshot-based testing and fast startup via restored KV cache.

# Key concepts

- `ChatSession`: `SaveSession(path)`, `LoadSession(path, loadTransforms:bool)`, `GetSessionState()` /
  `LoadSession(state, ...)`.
- `InteractiveExecutor` / executors: `SaveState(string)`, `GetStateData()`, `LoadState(byte[]/string)`.
- `LLamaContext`: `SaveState(string)` / `GetState()` for native context snapshots.

# Guidance

1. Separate blobs: store `historyText`, `executorState` (binary), `contextState` (binary), and `metadata.json`.
2. Metadata keys: `version`, `modelChecksum`, `n_ctx`, `createdUtc`, `engineVersion` — validate on load.
3. ISnapshotStore abstraction: implement `SaveAsync(id, SnapshotPackage)` and `LoadAsync(id)` to allow in-memory,
   filesystem or cloud-backed stores.
4. Atomic writes: write to temp files and rename to avoid partial state files.
5. Rotation/pruning: keep retention policy to limit storage growth.

# Example

Save:

- Call `executor.SaveState(file)` or `context.GetState()` and write bytes.
- Serialize chat history via `IHistoryTransform.HistoryToText()`.
- Write `metadata.json` with model id/checksum and `n_ctx`.

Load:

- Read `metadata.json`; if `modelChecksum` or `n_ctx` mismatch, refuse to load and surface an error.
- Load binary state and call `executor.LoadState(bytes)` and `context.LoadState(bytes)` as appropriate.
- Load history text and reconstruct `ChatHistory`, then call `session.LoadSession(state, loadTransforms:true)`.

## Safety & error handling

- Wrap loads in try/catch and keep the old working state if load fails.
- Avoid partial overwrites by writing to temporary files then renaming on success.
- Always verify compatibility (model checksum, `n_ctx`) before applying binary state to a running executor.

## Suggested interfaces

```csharp
public interface ISnapshotStore
{
    Task SaveAsync(string id, SnapshotPackage package);
    Task<SnapshotPackage?> LoadAsync(string id);
}

public record SnapshotPackage(byte[]? ExecutorState, byte[]? ContextState, string? HistoryText, SnapshotMetadata Metadata);
```

# References

- ChatSession save/load: https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/ChatSession/#saveload-chat-session
- Executor state save/load:
  https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/Executors/#save-and-load-executor-state
- Context & state: https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/UnderstandLLamaContext/
