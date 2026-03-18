# AtomicState Action Items

Date: 2026-03-18

Source: derived from `docs/internal/code-review-2026-03-18.md`

Purpose: capture concrete follow-up work from the design review without repeating the full review narrative.

## Working Rules

- keep changes proportionate to the current library size
- prefer pre-release API cleanup over adding new layers
- preserve the existing concurrency and dispatch guarantees unless a change explicitly intends to revise them
- validate build and non-performance tests after each completed action

## Priority Order

1. Preserve slice identity through composition and read access.
2. Narrow the middleware extension boundary.
3. Rename `ISlice.State` to reflect its actual lifecycle meaning.
4. Re-evaluate the `CombineReducers(...)` return shape only if action 1 changes the composition API.
5. Strengthen documentation around reducer identity rules.

## Status Board

- [x] Action 1: Preserve slice identity through the public API
- [x] Action 2: Narrow the middleware extension boundary
- [x] Action 3: Rename `ISlice.State` to `InitialState`
- [x] Action 4: Reviewed after action 1; no `CombineReducers(...)` return-shape change needed now
- [x] Action 5: Strengthen documentation for reducer identity rules

## Action 1: Preserve Slice Identity Through the Public API

Priority: high

Status: completed on 2026-03-18

Checklist:

- [x] design a stable slice token or equivalent access handle
- [x] update `Factory.CreateSlice(...)` and related composition flow to preserve slice identity
- [x] update combined-state access to use the new primary path
- [x] update example code and affected tests
- [x] validate build and non-performance tests

### Likely touch points

- `src/AtomicState/State/ISlice.cs`
- `src/AtomicState/Store/Factory.cs`
- `src/AtomicState/State/StateAccess.cs`
- `dev/AtomicState/ExampleStore.cs`
- `dev/AtomicState/Program.cs`
- `tests/AtomicState/StateAccessTests.cs`
- `tests/AtomicState/FactoryTests.cs`

### Verification

- slice access works without string literals in the main example path
- tests cover successful typed access and failure modes for unsupported root states or mismatched slice tokens
- `dotnet build *.sln`
- `dotnet test tests/*/*.Tests.csproj --filter "Category!=Perf"`

### Completion Notes

- `Factory.CreateSlice(...)` now returns `SliceHandle<TState>` so callers can carry slice identity from creation through
  access
- `StateAccess` now uses slice handles instead of raw string names as the supported public access path
- reducer composition keeps the existing internal string-keyed storage model while the public API becomes refactor-safe
- examples and tests now read combined slice state through the original handles instead of repeating string literals

## Action 2: Narrow the Middleware Extension Boundary

Priority: medium

Status: completed on 2026-03-18

Checklist:

- [x] decide the minimum middleware capabilities that must remain public
- [x] replace or narrow the middleware-facing store/context contract
- [x] preserve nested dispatch serialization semantics
- [x] update middleware behavior tests to match the revised contract
- [x] validate build and non-performance tests

### Likely touch points

- `src/AtomicState/Store/Dispatch.cs`
- `src/AtomicState/Store/IStore.cs` or a replacement middleware-specific context type
- `src/AtomicState/Internal/Store.cs`
- `tests/AtomicState/StoreTests.cs`
- `tests/AtomicState/StoreMiddlewareDispatchTests.cs`

### Verification

- middleware still supports nested dispatch in serialized order
- any remaining construction-time restrictions are explicit and minimal
- tests cover the allowed middleware behaviors directly
- `dotnet build *.sln`
- `dotnet test tests/*/*.Tests.csproj --filter "Category!=Perf"`

### Completion Notes

- middleware now receives `IDispatchContext` instead of `IStore`
- the store no longer carries activation guard logic just to protect middleware from unsupported store access
- nested dispatch and background dispatch middleware tests were preserved under the narrowed contract
- composition-time guard tests were removed because the new API prevents those misuse cases by construction
- the README and store factory documentation now describe dispatch-only middleware context

## Action 3: Rename `ISlice.State` to `InitialState`

Priority: medium-low

Status: completed on 2026-03-18

Checklist:

- [x] rename `ISlice.State` to `ISlice.InitialState`
- [x] update the internal slice implementation and composition path
- [x] update affected tests
- [x] update README wording for the renamed member
- [x] validate build and non-performance tests

### Likely touch points

- `src/AtomicState/State/ISlice.cs`
- `src/AtomicState/Internal/StateSlice.cs`
- `src/AtomicState/Store/Factory.cs`
- `tests/AtomicState`
- `README.md`

### Verification

- no remaining public references imply that slice state is mutable or current
- examples still read clearly after the rename
- `dotnet build *.sln`
- `dotnet test tests/*/*.Tests.csproj --filter "Category!=Perf"`

### Completion Notes

- `ISlice.State` was renamed to `ISlice.InitialState`
- the internal slice implementation and reducer composition path now use the renamed member
- factory and store tests were updated to use `InitialState`
- the README now explicitly distinguishes `ISlice.InitialState` from `IStore.State`

## Action 4: Revisit the `CombineReducers(...)` Return Shape If Action 1 Expands Composition Metadata

Priority: low

Status: reviewed on 2026-03-18; no change needed now

Checklist:

- [x] decide whether action 1 actually requires additional composition metadata
- [x] confirm the current tuple remains sufficient after action 1
- [x] defer any named result type until composition returns more than root state and reducer

### Likely touch points

- `src/AtomicState/Store/Factory.cs`
- `tests/AtomicState/FactoryTests.cs`
- `README.md`

### Verification

- the composition result is easier to understand than the tuple it replaces
- the type adds clear value instead of ceremony
- `dotnet build *.sln`
- `dotnet test tests/*/*.Tests.csproj --filter "Category!=Perf"`

### Review Notes

- action 1 introduced `SliceHandle<TState>` as the stable public slice identity, so composition no longer needs to
  return slice-access metadata separately
- the current `(state, reducer)` tuple still matches the actual consumer flow for `Factory.CreateStore(...)`
- a named composition result should only be introduced if `CombineReducers(...)` later needs to return an additional
  public concept beyond root state and reducer
- no code, tests, or README updates are needed for action 4 at this time

## Action 5: Strengthen Documentation for Reducer Identity Rules

Priority: low

Status: completed on 2026-03-18

Checklist:

- [x] review README sections that describe reducer behavior
- [x] ensure examples consistently return the existing instance for no-op actions
- [x] strengthen any XML documentation that understates the identity rule
- [x] verify docs remain aligned with the implemented API shape

### Likely touch points

- `README.md`
- `src/AtomicState/State/Reducer.cs`
- `src/AtomicState/Store/Factory.cs`
- any example code that demonstrates reducers

### Verification

- docs state the rule plainly and consistently
- examples align with the documented rule

### Completion Notes

- the README now states that reducer identity is part of the library's change-detection contract rather than a style
  preference
- the README examples and architecture notes now explicitly reinforce returning the existing instance for logical no-op
  actions
- reducer delegate XML docs now state that equal-but-new state objects still count as changes because the library uses
  reference-based detection
- README wording was updated to stay aligned with the current handle-based slice access API

## Completion Criteria

- [x] string-based slice access is no longer the primary supported consumer path
- [x] the middleware contract reflects actual supported behaviors instead of exposing a larger store surface than needed
- [x] the slice initial-state member is clearly named
- [x] composition output remains understandable after the access redesign
- [x] documentation matches the final reducer and slice contracts
