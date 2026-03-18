# AtomicState Design Review

Date: 2026-03-18

Scope: review of the public and internal design around store composition, slice access, middleware boundaries, and
state-stream behavior in `src/AtomicState`, with supporting checks in `tests/AtomicState` and `dev/AtomicState`.

## Findings

### 1. Slice identity is lost after composition

Severity: high

`Factory.CreateSlice(...)` and `ISlice` establish slices as first-class building blocks, but consumers cannot carry that
identity through to read access. After `Factory.CombineReducers(...)`, the supported read path is
`StateAccess.GetSliceState<TState>(rootState, sliceName)`, which falls back to string keys.

Why this matters:

- slice names become runtime-only contracts instead of refactor-safe handles
- the library pays the abstraction cost of `ISlice` without preserving a stable consumer-facing token
- common usage already exposes the weakness in consumer code and tests

Relevant code:

- `src/AtomicState/Store/Factory.cs`
- `src/AtomicState/State/ISlice.cs`
- `src/AtomicState/State/StateAccess.cs`
- `dev/AtomicState/Program.cs`
- `tests/AtomicState/StateAccessTests.cs`

Recommended direction:

Preserve slice identity in the public API. A typed slice handle, accessor object, or similar token would remove the
stringly-typed lookup path without adding a large new architecture.

### 2. Middleware receives a broader contract than the extension point needs

Severity: medium

The middleware contract accepts `IStore`, which exposes state reads, subscriptions, dispatch, and disposal. That broad
surface forces the runtime to protect construction with `MiddlewareStore` and throw if middleware touches the store
before activation.

Why this matters:

- the runtime is compensating for an oversized extension boundary
- middleware authors must understand a lifecycle rule that exists because the interface is wider than the composition
  phase can safely support
- giving middleware `Dispose` and subscription access couples one extension seam to nearly the full store contract

Relevant code:

- `src/AtomicState/Store/Dispatch.cs`
- `src/AtomicState/Store/IStore.cs`
- `src/AtomicState/Internal/Store.cs`
- `tests/AtomicState/StoreTests.cs`
- `tests/AtomicState/StoreMiddlewareDispatchTests.cs`

Recommended direction:

Consider a narrower middleware context before release. Dispatch re-entry is clearly needed, and state reads may be
justified, but full lifecycle ownership and the full observable surface do not look essential to the extension seam.

### 3. `ISlice.State` is named like current state but behaves like initial state

Severity: low

`ISlice.State` and `IStore.State` use the same member name for different lifecycle meanings. On `ISlice`, the property
is only the initial snapshot consumed during combination. On `IStore`, it is the current committed snapshot.

Why this matters:

- the naming makes the slice contract less self-explanatory than it needs to be
- the ambiguity is small but public, so it affects discoverability and API reading cost

Relevant code:

- `src/AtomicState/State/ISlice.cs`
- `src/AtomicState/Store/IStore.cs`
- `src/AtomicState/Store/Factory.cs`

Recommended direction:

Rename the slice property to something explicit such as `InitialState` while the package is still pre-release.

## Acceptable Trade-Offs

### Minimal core contracts fit the current library size

The sparseness of `IAction`, `IState`, and the reducer delegates is acceptable for this repository. They do not
currently look like under-designed placeholders so much as intentionally small contracts for a small state library.

### `StateStream` complexity is justified by the contract it claims

The internal delivery logic is intricate, but it buys a concrete guarantee: replay, live publication, completion, and
disposal are observed in one linearized order. That is a hard problem, and the tests cover the race boundaries well
enough to justify the complexity.

### Internal `@@INIT` dispatch is a reasonable construction rule

Running initialization as part of store creation is a sound fit for this model. The behavior is documented, tests cover
reducer and middleware failures during initialization, and the rule prevents partially initialized stores from escaping.

## Optional Improvements

### Replace the tuple return from `CombineReducers(...)` if slice identity is redesigned

The tuple return shape is acceptable today, but it is the natural place to introduce a named composition result if the
library later decides to preserve slice identity or expose richer combined-state metadata.

### Keep emphasizing the reducer identity contract in docs

Reference-based change detection is consistent across the store and reducer composition path, but it remains a
behavioral convention rather than a type-enforced rule. That makes documentation quality part of the design, not just
auxiliary material.

## Validation

- `dotnet build *.sln`: passed
- `dotnet test tests/*/*.Tests.csproj --filter "Category!=Perf"`: passed

## Reviewed Areas

- `src/AtomicState/Actions`
- `src/AtomicState/State`
- `src/AtomicState/Store`
- `src/AtomicState/Internal`
- `tests/AtomicState`
- `dev/AtomicState`
