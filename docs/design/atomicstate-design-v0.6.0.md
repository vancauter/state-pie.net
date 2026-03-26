# AtomicState Design v0.6.0

Status: Draft — pre-release (breaking changes allowed)

# Index

- [Purpose](#purpose)
- [Design principles](#design-principles)
- [TypedAction helpers and small codegen](#typedaction-helpers-and-small-codegen)
- [Clearer examples and tests for identity-preservation](#clearer-examples-and-tests-for-identity-preservation)
- [Persistence adapter: ISnapshotStore and snapshot test doubles](#persistence-adapter-isnapshotstore-and-snapshot-test-doubles)
- [Async dispatch safety and middleware guidance](#async-dispatch-safety-and-middleware-guidance)
- [Metrics hooks and lightweight telemetry API](#metrics-hooks-and-lightweight-telemetry-api)
- [Snapshot / seed helper for deterministic tests and replay harnesses](#snapshot-seed-helper-for-deterministic-tests-and-replay-harnesses)
- [Implementation plan (high level)](#implementation-plan-high-level)
- [Files and locations suggested](#files-and-locations-suggested)
- [Open questions / trade-offs](#open-questions-trade-offs)
- [Acceptance criteria (for v0.6.0)](#acceptance-criteria-for-v060)
- [Architecture & project structure recommendations](#architecture-and-project-structure-recommendations)
- [TimeTravel middleware](#timetravel-middleware)
- [References](#references)

## Purpose

Present a concise, implementable design covering all requested ergonomics, testing, observability, tooling improvements,
and a curated set of runnable examples:

- TypedAction helpers and minimal codegen for slice/action types to reduce boilerplate and accidental type-string bugs.
- Clearer examples and tests demonstrating the identity-preservation rule and common reducer anti-patterns.
- A lightweight persistence adapter pattern (`IPersistMiddleware`) and test doubles for `ISnapshotStore` to simplify
  middleware tests.
- Async-middleware patterns and guidance for safe nested dispatches, plus an explicit async-dispatch API and helpers
  (e.g., `DispatchAsync`, `Enqueue`, `MiddlewareHelpers.ScheduleBackground`, `DispatchContext.SafeDispatchAsync`) and
  debug-time guards to detect common misuse.
- TimeTravel middleware: bounded history, undo/redo, replay harnesses, and a control surface for devtools and tests.
- Metrics hooks (dispatch throughput, middleware latency) and a small store snapshot/seed helper for deterministic
  integration tests and deterministic replay harnesses.
- Examples: create an `./examples` directory at the repository root with subdirectories containing concrete, runnable
  examples (usage, middleware, persistence, TimeTravel, async-middleware, metrics) and README guides.

- Documentation alignment: update `README.md` and `.github/skills/atomic-state/SKILL.md` so public-facing docs and
  integration guidance match the implemented public API names (for example, `TimeTravel` middleware naming and
  control-surface types). This is a final documentation task performed after implementation changes.

Note: The repository now contains a top-level `examples/` directory with `.cs` snippets demonstrating basic usage,
typed-actions, reducer identity guidance, flat slices vs nested slices (with guidance), middleware scheduling,
persistence, time-travel, and a metrics example. The earlier ad-hoc `dev/Examples` project was removed in favor of these
top-level snippets; examples are numbered 01 (basic) → 09 (advanced) for easy discovery.

## Design principles

- Keep the core store contract unchanged: reducers must still return the same reference on no-op.
- New helpers are opt-in: consumers adopt typed actions, persistence, metrics, or async patterns as needed.
- Provide deterministic test doubles and seed helpers to avoid brittle integration tests.
- Document safe patterns and anti-patterns around nested dispatch to avoid deadlocks or surprising interleavings.

## TypedAction helpers and small codegen

### Goal

Reduce runtime string-typing mistakes and make payloads easier to reason about.

### API sketches

- `record TypedActionId<T>(string Type);`
- `IAction<T>` already exists; add helpers in `Factory`:
    - `static IAction CreateAction(string type)` (existing)
    - `static IAction<T> CreateAction<T>(TypedActionId<T> id, T payload)`

### Usage example

var SetName = new TypedActionId<string>("@PERSON/SET_NAME"); var action = Factory.CreateAction(SetName, "Lee-Lee");

### Reducer pattern

return action.Type switch { _ when action is IAction<string> pa && pa.Type == SetName.Type => current with { Name =
pa.Payload }, _ => state };

### Codegen

- Provide an optional tiny generator (`tools/atomicstate.codegen` CLI or dotnet tool) that reads a config file and emits
  a small `Actions.cs` / `Slices.cs` file containing `TypedActionId<T>` constants and `SliceHandle<TState>` constants
  for a project. Generator must be optional — handwritten constants work fine.

### Justification

- Centralizes type strings, reduces typos, and keeps payload typing explicit. Does not change runtime semantics.

## Clearer examples & tests for identity-preservation

### Deliverables

- New tests under `tests/AtomicState/Examples`:
    - `Reducer_NoOp_PreservesReference` — verifies no publication when reducer returns `state`.
    - `Reducer_NewButValueEqual_TriggersChange` — demonstrates the anti-pattern and documents why it's incorrect.
    - Example-driven README snippets moved into executable tests.

- `docs/reducer-guidelines.md` — short rules and minimal code snippets documenting the identity contract.

## Persistence adapter: `ISnapshotStore` and snapshot test doubles

### Design goals

- Provide a small, pluggable persistence middleware interface that can snapshot state and restore on initialization.
- Make it easy to write unit tests by providing `InMemorySnapshotStore` and `TestSnapshotStore` doubles.

### API sketches

- `public interface ISnapshotStore { Task SaveAsync(string key, IState snapshot, CancellationToken ct = default); Task<IState?> LoadAsync(string key, CancellationToken ct = default); }`

- `public delegate Task PersistMiddlewareDelegate(IDispatchContext context, IAction action, Func<IAction, Task> next);`
  — middleware receives the action and may trigger a snapshot after `next(action)` completes.

- Convenience: `PersistMiddleware.Create(ISnapshotStore store, string key, SnapshotOptions? options = null)` returns a
  `Middleware` delegate that saves snapshots after successful dispatches, and can optionally load an initial snapshot at
  store creation time (via a helper `StoreSeedHelper` — see below).

### Test doubles

- `InMemorySnapshotStore` — thread-safe, fast, deterministic store for tests.
- `ThrowingSnapshotStore` — simulates failures for error-path tests.

## Async dispatch safety and middleware guidance

### Problem summary

- Middleware may dispatch nested actions. Synchronous nested dispatch participates in the store's serialized pipeline
  and is a supported pattern. The common hazard arises when middleware performs asynchronous work and then dispatches
  while callers remain synchronously blocked or when middleware awaits inside the dispatch pipeline without yielding.
  That mix can produce surprising ordering, deep stack chains, or deadlocks if callers block the dispatching thread.

### Comparison to industry practice

- This is a well-known trade-off in reducer-based systems (Redux, similar libraries). Industry practice is to permit
  synchronous nested dispatch but encourage async work to be scheduled off the dispatch stack (thunks, sagas, background
  tasks). The issue is not unique to AtomicState but benefits from explicit rules and helpers.

### Design changes (grouped)

- Document the rule and provide explicit examples (safe vs unsafe) so consumers learn the patterns by example.
- Add an explicit async dispatch API on the dispatch context that makes background intent explicit and opt-in:
    - `ValueTask DispatchAsync(IAction action)` or `Task Enqueue(IAction action)` — both available; `Dispatch` remains
      synchronous for transactional/nested use.
- Provide helper utilities to schedule background work safely:
    - `MiddlewareHelpers.ScheduleBackground(Func<Task> work)` — schedules `work` via `Task.Run` and applies recommended
      yields.
    - `DispatchContext.SafeDispatchAsync(IDispatchContext ctx, IAction action)` — helper that uses
      `Task.Yield`/`Task.Run` to avoid blocking the pipeline.
- Add debug-time runtime guards (debug-only): detect common misuse and surface diagnostics:
    - Warn when middleware awaits a non-completed task inside the dispatch pipeline (heuristic using callstack / task
      state).
    - Detect blocking waits on the dispatch thread (calls to `.Result`/`.Wait()` on the dispatch task) and log a helpful
      diagnostic.

### API sketches

- Extend `IDispatchContext` (or provide an adapter) with:
    - `ValueTask DispatchAsync(IAction action, CancellationToken ct = default);`
    - `Task Enqueue(IAction action, CancellationToken ct = default);` (alternative async enqueue semantics)

- Add `public static class MiddlewareHelpers` with:
    - `public static void ScheduleBackground(Func<Task> work)` — schedules with `Task.Run` and `Task.Yield` for safety.
    - `public static Task SafeDispatchAsync(IDispatchContext ctx, IAction action)` — uses `Task.Yield` then calls
      `ctx.Dispatch(action)` or `ctx.DispatchAsync(action)` depending on availability.

### Examples (safe vs unsafe)

- Unsafe (await inside dispatch without yielding):

    var bad = Middleware.Create((ctx) => (action) => { await DoWorkAsync(); // awaiting here may block the dispatch
    pipeline ctx.Dispatch(Factory.CreateAction("@A/AFTER")); return ctx.Dispatch(action); });

- Safe (schedule background work):

    var good = Middleware.Create((ctx) => (action) => { MiddlewareHelpers.ScheduleBackground(async () => { await
    DoWorkAsync(); await DispatchContext.SafeDispatchAsync(ctx, Factory.CreateAction("@A/AFTER")); }); return
    ctx.Dispatch(action); });

### Debug-time guards

- In debug builds provide optional runtime checks wired into `StoreOptions.DebugGuards`:
    - `WarnOnAwaitInDispatch` — detect and log when middleware awaits inside the dispatch pipeline.
    - `DetectBlockingWaits` — monitor for synchronous blocking on dispatch tasks and log a helpful stacktrace.

### Testing & docs

- Add unit tests demonstrating both safe and unsafe patterns and show recommended replacements.
- Update `docs/async-middleware.md` with clear, minimal examples and rationale.

### Recommendation and rollout

- Immediate: add docs/examples and `MiddlewareHelpers` + `DispatchContext.SafeDispatchAsync` helpers.
- Short-term: add `DispatchAsync` on the dispatch context and unit tests demonstrating expected behavior.
- Optional: enable debug-time guards behind `StoreOptions.DebugGuards` to help catch misuses early during development.

## Metrics hooks and lightweight telemetry API

### Goals

- Expose optional hooks for instrumentation without forcing a telemetry dependency.

API sketch

- `public interface IStoreMetrics { void OnDispatchStart(); void OnDispatchEnd(TimeSpan elapsed); void OnMiddlewareStart(string middlewareId); void OnMiddlewareEnd(string middlewareId, TimeSpan elapsed); }`

- `NoOpStoreMetrics` default implementation does nothing. Consumers provide implementations that forward to their
  telemetry systems.

- `Factory.CreateStore(..., StoreOptions options = null)` accepts `StoreOptions.Metrics` for wiring metrics at creation
  time.

### Reported metrics

- Dispatch throughput (number of dispatches per second) — can be computed from OnDispatchEnd timestamps.
- Per-middleware latency distribution — via OnMiddlewareStart/OnMiddlewareEnd.

## Snapshot / seed helper for deterministic tests and replay harnesses

### API

- `public static class StoreSnapshotHelper` with:
    - `static Task SeedStoreAsync(IStore store, IState snapshot)` — applies snapshot without going through dispatch
      (internal helper for tests)
    - `static Task<IState> CaptureSnapshotAsync(IStore store)` — returns current root state
    - `static IStore CreateSeededStore(IState seedState, Reducer rootReducer, StoreOptions? options = null)` —
      convenience for integration tests

### Implementation notes

- `SeedStoreAsync` should prefer an internal store method that sets the initial state without firing lifecycle events,
  preserving identity rules. If that internal helper is not available, provide a test-only factory that constructs a
  store pre-seeded via `Factory.CreateStore(seedState, rootReducer, ...)`.

### Tests & verification

- Add unit tests:
    - TypedAction: compile-time usage and runtime dispatch with payloads.
    - Persistence middleware: `InMemorySnapshotStore` behavior, restore and save flows, and failure handling with
      `ThrowingSnapshotStore`.
    - Async-middleware: ensure nested synchronous dispatch ordering and show that background-scheduled dispatches are
      serialized but asynchronous.
    - Metrics: verify `IStoreMetrics` hooks are invoked and timing is sensible under simple workloads.
    - Snapshot helpers: ensure `CreateSeededStore` produces deterministic state and `CaptureSnapshotAsync` returns the
      committed state.

## Implementation plan (high level)

1. Add `TypedActionId<T>` and `Factory.CreateAction<T>(TypedActionId<T>, T)` helpers; add unit tests.
2. Add `tests/AtomicState/Examples` tests and `docs/reducer-guidelines.md` with identity-preservation examples.
3. Introduce `ISnapshotStore` and `PersistMiddleware` helper plus `InMemorySnapshotStore` and `ThrowingSnapshotStore`
   doubles; add tests.
4. Add `StoreSnapshotHelper` and `CreateSeededStore` convenience; wire into tests.
5. Add `IStoreMetrics` interface, default `NoOpStoreMetrics`, and metrics wiring in `StoreOptions` and
   `Factory.CreateStore`.
6. Author `docs/async-middleware.md` with safe patterns and examples; add tests showing correct ordering and no
   deadlocks.
7. Provide optional `atomicstate.codegen` minimal generator for typed constants (separate tiny tool / sample
   implementation in `dev/tools`).

## Files and locations suggested

- `src/AtomicState/Helpers/TypedActionId.cs`
- `src/AtomicState/Helpers/Factory.TypedAction.cs` (new overloads)
- `src/AtomicState/Adapters/Persistence/PersistMiddleware.cs`
- `src/AtomicState/Persistence/ISnapshotStore.cs` and `InMemorySnapshotStore.cs`
- `src/AtomicState/Helpers/StoreSnapshotHelper.cs`
- `src/AtomicState/Telemetry/IStoreMetrics.cs` and `NoOpStoreMetrics.cs`
- `tests/AtomicState/Examples/*` — new example tests
- `docs/reducer-guidelines.md`, `docs/async-middleware.md`, `docs/persistence.md`

## Open questions / trade-offs

- Namespace placement: keep helper APIs in `SC.AtomicState.Helpers` or `SC.AtomicState.Experimental` until adoption.
- Codegen delivery: offer a very small, optional codegen tool vs. accept handwritten constants. Prefer optional tool to
  keep core small.

## Acceptance criteria (for v0.6.0)

- TypedAction helpers implemented and covered by unit tests.
- Example tests showing reducer identity rules and anti-patterns.
- `ISnapshotStore` + `PersistMiddleware` implemented with in-memory and failure doubles and covered by tests.
- `StoreSnapshotHelper` and `CreateSeededStore` available for deterministic integration tests.
- Async-middleware guidance documented and tested (no deadlocks in sample tests).
- `IStoreMetrics` API present and wired through `Factory.CreateStore`.
- `README.md` and `.github/skills/atomic-state/SKILL.md` updated to reflect implemented public API names (for example
  `TimeTravel` middleware) and usage guidance.
- Examples: curated `examples/` snippets for basic, middleware, persistence, time-travel, async-middleware, and metrics
  exist and are runnable via the provided `csproj`s.
- Status alignment: the implementation status document (`docs/design/atomicstate-design-v0.6.0-status.md`) is updated to
  reflect the current progress against these acceptance criteria.

## Implementation notes for maintainers

- Keep changes minimal and opt-in. The design intentionally avoids changes to the dispatch serialization semantics.
- Where feasible, mark new helpers as `public` but consider placing them under `Helpers` or `Experimental` namespaces
  until validated.

## Architecture & project structure recommendations

### Goals

- Keep a minimal, stable public surface for consumers.
- Make testing and examples easy to run (executable examples with their own `csproj`).
- Separate experimental helpers from the core runtime to avoid accidental API surface growth.
- Follow Microsoft library design guidance and .NET packaging patterns so other teams can consume the library easily.

### Concrete changes

#### Projects and folders

    - `src/AtomicState/` (core runtime) — keep core surface small: `Factory`, `IStore`, `Store`, `Reducer` wiring.
    - `src/AtomicState.Middleware/` — middleware implementations (Logging, Persist, TimeTravel) and middleware factory
      helpers.
    - `src/AtomicState.Persistence/` — `ISnapshotStore`, `InMemorySnapshotStore`, persistence adapters.
    - `src/AtomicState.Helpers/` — `TypedActionId<T>`, `MiddlewareHelpers`, `StoreSnapshotHelper`, `DispatchContext`
      helpers.
    - `src/AtomicState.Telemetry/` — `IStoreMetrics`, `NoOpStoreMetrics`, telemetry adapters.
    - `src/AtomicState.Experimental/` — early, opt-in helpers and generators (codegen samples). Move to main namespaces
      after vetting.
    - `dev/tools/atomicstate.codegen/` — optional tiny codegen CLI that emits `Actions.cs` and `Slices.cs` for projects.
    - `examples/` — curated runnable examples (see Examples section below).

- Namespaces and naming
    - Use `SC.AtomicState` as the root public namespace. Sub-namespaces mirror folders: `.Middleware`, `.Persistence`,
      `.Helpers`, `.Telemetry`, `.Experimental`.
    - Public API naming: `TypedActionId<T>`, `ITimeTravelController`, `TimeTravelMiddleware`, `ISnapshotStore`,
      `MiddlewareHelpers`, `StoreSnapshotHelper`, `IStoreMetrics`.
    - Dispatch API: extend `IDispatchContext` (or an adapter) with
      `ValueTask DispatchAsync(IAction action, CancellationToken ct = default)` and
      `Task Enqueue(IAction action, CancellationToken ct = default)`.

- DI and hosting integration
    - Provide DI registration extensions: `IServiceCollection.AddAtomicState(Action<AtomicStateOptions>?)` that wires
      `IStore`, `ISnapshotStore` defaults, `IStoreMetrics` and middleware wiring.
    - For dev harnesses (dev/\*), prefer the .NET Generic Host pattern (`Host.CreateDefaultBuilder`) to expose lifetime,
      logging, and DI in a standard way (see ASP.NET Core fundamentals guidance).

    - Remove `SimpleLog` usage: replace the repository's `SimpleLog` project reference and any direct logging calls with
      the industry-standard `Microsoft.Extensions.Logging` abstractions. Migration guidance:
        - Add references to `Microsoft.Extensions.Logging.Abstractions` (and consumers may add
          `Microsoft.Extensions.Logging` or provider packages as needed).
        - Replace direct `SimpleLog` usages with `ILogger<T>` injected via DI (add `IServiceCollection.AddLogging()` in
          host examples and `IServiceCollection.AddAtomicState()` extension).
        - Remove the `SimpleLog` project reference from `AtomicState.sln` and update developer docs/README to note the
          change and recommended logging providers (Console, EventSource, or structured providers used by consumers).

- Testing and packaging
    - Keep `InternalsVisibleTo` to tests for internal contract testing (already used in csproj).
    - Use `Directory.Packages.props` for central package versions if multiple projects are added.
    - Include `examples/*` with self-contained `csproj`s that reference `src/AtomicState/AtomicState.csproj` by project
      reference so users can run examples locally with `dotnet run`.

### Examples directory (recommended structure)

examples/

- basic/ # minimal create-store/dispatch example
- middleware/ # examples showing persist, logging middleware
- async-middleware/ # safe vs unsafe patterns, DispatchAsync usage
- TimeTravel/ # demo harness showing undo/redo + replay
- persistence/ # snapshot store examples and usage
- metrics/ # metrics wiring and sample consumer

Each example folder should include a short README and a simple `Program.cs` demonstrating the feature.

### Why these changes

- Aligns the repo with Microsoft guidance and popular reference patterns (single responsibility per project, DI-friendly
  services, examples and sample apps colocated in `examples/`).
- Improves discoverability: consumers find persistence and middleware in their own namespaces and projects.
- Makes experimental work safe: new APIs live in `Experimental` until adoption.

### Migration notes

- These are additive and organizational. Existing code can remain in `src/AtomicState` while new folders are introduced.
- Public API shape should remain stable; only introduce new overloads and opt-in helpers. Mark experimental types with
  an `Experimental` remark in XML docs.

## TimeTravel middleware

### Purpose

Provide an optional middleware that records a bounded history of committed root-state snapshots (or diffs) and exposes
undo/redo and deterministic replay controls for testing, debugging, and interactive developer tooling.

Design goals

- Non-invasive: middleware must not change store serialization or reducer semantics.
- Bounded and configurable: history size, retention policy, snapshot compression (full vs diff), and memory budget are
  configurable via `TimeTravelOptions`.
- Deterministic replay: replay applies the original actions (preferred) or recorded snapshots to recreate state.
- Testable: provide test doubles and deterministic helpers to assert history contents and replay behavior.

API sketches

- `public record TimeTravelOptions(int MaxHistory = 100, bool StoreActions = true, bool StoreSnapshots = false, bool UseDiffs = true);`
- `public interface ITimeTravelController` — programmatic control surface:
    - `Task UndoAsync()`
    - `Task RedoAsync()`
    - `Task ReplayAsync(IEnumerable<IAction> actions)`
    - `IReadOnlyList<IState> GetHistory()`
    - `void Clear()`

- Middleware creator: `TimeTravelMiddleware.Create(TimeTravelOptions options, out ITimeTravelController controller)`
  returns a `Middleware` delegate and an attached controller instance.

Behavior notes

- Recording: by default the middleware records the action sequence and a compact snapshot (or diff) after each committed
  dispatch completes successfully. If `StoreActions == true` the original `IAction` objects (or stable serializations)
  are stored and used for deterministic replay; otherwise snapshots/diffs are used.
- Undo/Redo: `UndoAsync` dispatches a synthesized `@@TIMETRAVEL/UNDO` action that the middleware intercepts and, under
  the store's serialization, replaces the current state with the previous snapshot (or replays actions to recreate
  state) without re-recording the undo/redo itself in history.
- Replay: `ReplayAsync` replays a sequence of recorded or externally supplied actions into a fresh seeded store (use
  `StoreSnapshotHelper.CreateSeededStore` to create deterministic replay harnesses).
- Memory and performance: when `UseDiffs==true` middleware stores diffs between snapshots to reduce memory; diff
  algorithm must be tuned for record types (shallow member diffs are usually sufficient) and remain conservative to
  preserve identity semantics.

Integration points

- Metrics: middleware should call `IStoreMetrics.OnMiddlewareStart/End` with an id like `"timetravel"`.
- Persistence: optionally persist history via `ISnapshotStore` or consumer-provided store for longer retention across
  runs.
- UI/Devtools: `ITimeTravelController` can be exposed to debugging UIs to present a timeline and allow stepping.

Safety & identity rules

- When applying snapshots/undos, the middleware must set the store's committed `IState` instance to the exact stored
  instance (or a reconstructed instance) so identity-based change detection behaves correctly.
- Undo/Redo operations must not themselves be recorded in history (middleware should suppress recording while performing
  time-travel operations).

Testing

- Unit tests:
    - `TimeTravel_RecordsActionsAndSnapshots` — verifies history entries after dispatches.
    - `TimeTravel_UndoRestoresPreviousStateReference` — verifies identity preservation after undo.
    - `TimeTravel_RedoRestoresForwardState` — verifies redo behavior.
    - `TimeTravel_ReplayProducesDeterministicState` — seeds a fresh store and replays recorded actions, verifying final
      state matches original.
    - `TimeTravel_BoundedHistoryEvictsOldEntries` — verifies `MaxHistory` enforcement.

- Integration tests:
    - Combine `TimeTravelMiddleware` with `PersistMiddleware` and `InMemorySnapshotStore` to validate persistence and
      replay across lifecycles.

Acceptance criteria

- `TimeTravelMiddleware.Create(...)` exists and returns a `Middleware` plus `ITimeTravelController`.
- Undo/redo operations restore exact state instances (or logically equal reconstructed instances) and preserve reducer
  identity semantics.
- Replay harness produces deterministic state matches when replaying recorded actions into a seeded store.
- Tests above are present and pass.

## References

- README.md — current library contracts and examples
- Existing tests under `tests/AtomicState` for inspiration and style
