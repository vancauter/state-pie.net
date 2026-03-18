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

- [ ] Action 1: Preserve slice identity through the public API
- [ ] Action 2: Narrow the middleware extension boundary
- [x] Action 3: Rename `ISlice.State` to `InitialState`
- [ ] Action 4: Revisit the `CombineReducers(...)` return shape if action 1 expands composition metadata
- [ ] Action 5: Strengthen documentation for reducer identity rules

## Action 1: Preserve Slice Identity Through the Public API

Priority: high

Checklist:

- [ ] design a stable slice token or equivalent access handle
- [ ] update `Factory.CreateSlice(...)` and related composition flow to preserve slice identity
- [ ] update combined-state access to use the new primary path
- [ ] update example code and affected tests
- [ ] validate build and non-performance tests

### Goal

Remove string-only slice lookup from the intended consumer path so slice access is refactor-safe and stays aligned with
the slice abstraction already exposed by the API.

### Why this action exists

The current design makes slices first-class at creation time and anonymous at read time. That means `ISlice` carries
setup-time value, but consumers still depend on raw string keys after composition.

### Expected outcome

- consumers can read slice state without manually repeating the slice name as a string
- the public API keeps one stable identity for a slice from definition through lookup
- example code and tests no longer need string-based access for normal usage

### Suggested implementation direction

- introduce a typed slice handle, accessor object, or equivalent public token
- ensure `Factory.CreateSlice(...)` returns or exposes that token in a way consumers can carry forward
- update the combined-state access path to accept the stable token rather than the slice name string for the primary API
- keep a compatibility bridge only if needed for transition, since the repository is still pre-release

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

## Action 2: Narrow the Middleware Extension Boundary

Priority: medium

Checklist:

- [ ] decide the minimum middleware capabilities that must remain public
- [ ] replace or narrow the middleware-facing store/context contract
- [ ] preserve nested dispatch serialization semantics
- [ ] update middleware behavior tests to match the revised contract
- [ ] validate build and non-performance tests

### Goal

Reduce the amount of store surface exposed to middleware so the extension point reflects what middleware actually needs
and no longer depends on broad lifecycle access.

### Why this action exists

Middleware currently receives `IStore`, which is larger than the composition phase can safely support. The runtime
compensates with guard logic and construction-time exceptions.

### Expected outcome

- middleware gets only the capabilities it needs
- construction semantics become easier to explain
- the design relies less on “this interface exists, but you cannot use all of it yet” behavior

### Suggested implementation direction

- decide which middleware capabilities are actually required: likely dispatch re-entry, possibly current-state reads
- remove disposal and subscription access from the middleware-facing context unless a concrete use case justifies them
- keep nested dispatch serialization semantics unchanged
- update tests to assert the intended middleware contract instead of only the current guard behavior

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

## Action 3: Rename `ISlice.State` to `InitialState`

Priority: medium-low

Status: completed on 2026-03-18

Checklist:

- [x] rename `ISlice.State` to `ISlice.InitialState`
- [x] update the internal slice implementation and composition path
- [x] update affected tests
- [x] update README wording for the renamed member
- [x] validate build and non-performance tests

### Goal

Make the slice contract self-explanatory by naming the property after its actual role.

### Why this action exists

`ISlice.State` looks like current state, but it is only the initial snapshot used during reducer composition. That is
easy to misunderstand next to `IStore.State`.

### Expected outcome

- slice and store contracts use distinct names for distinct lifecycle meanings
- API reading cost goes down for new consumers
- documentation can describe slice composition with less qualification

### Suggested implementation direction

- rename `ISlice.State` to `InitialState`
- propagate the rename through the internal slice implementation, factory composition logic, example code, and tests
- update XML documentation and any markdown references that mention the old member name

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

Checklist:

- [ ] decide whether action 1 actually requires additional composition metadata
- [ ] replace the tuple only if a named result type adds clear value
- [ ] update affected tests and docs if the return shape changes
- [ ] validate build and non-performance tests

### Goal

Avoid forcing additional composition metadata into an anonymous tuple if slice identity or access metadata becomes part
of the public composition result.

### Why this action exists

The tuple return is acceptable today, but it may become awkward if preserving slice identity introduces new public data
that logically belongs to the composition result.

### Expected outcome

- the API remains clear if composition starts returning more than root state and reducer
- the repository avoids premature type creation unless action 1 makes it necessary

### Suggested implementation direction

- only take this action if action 1 materially changes the shape of composition output
- if needed, replace the tuple with a named result type that keeps state, reducer, and any stable access metadata
  together

### Likely touch points

- `src/AtomicState/Store/Factory.cs`
- `tests/AtomicState/FactoryTests.cs`
- `README.md`

### Verification

- the composition result is easier to understand than the tuple it replaces
- the type adds clear value instead of ceremony
- `dotnet build *.sln`
- `dotnet test tests/*/*.Tests.csproj --filter "Category!=Perf"`

## Action 5: Strengthen Documentation for Reducer Identity Rules

Priority: low

Checklist:

- [ ] review README sections that describe reducer behavior
- [ ] ensure examples consistently return the existing instance for no-op actions
- [ ] strengthen any XML documentation that understates the identity rule
- [ ] verify docs remain aligned with the implemented API shape

### Goal

Keep the reference-based change detection contract visible enough that users do not accidentally defeat it by always
allocating new state for no-op actions.

### Why this action exists

This behavior is central to the library’s design, but it is enforced by convention rather than the type system.

### Expected outcome

- the main docs explain the rule where users will encounter it
- examples consistently model returning the existing state instance for no-op actions
- the contract stays clear without adding framework machinery to enforce it

### Suggested implementation direction

- review README sections that describe reducers, composition, and observing state
- ensure examples show no-op branches returning the existing state object
- update XML docs where the current wording is too subtle for a core behavior rule

### Likely touch points

- `README.md`
- `src/AtomicState/State/Reducer.cs`
- `src/AtomicState/Store/Factory.cs`
- any example code that demonstrates reducers

### Verification

- docs state the rule plainly and consistently
- examples align with the documented rule

## Recommended Sequence

1. Complete action 1 first, because it is the most significant public-surface issue.
2. Use the result of action 1 to decide whether action 4 is necessary.
3. Complete action 2 before release if middleware is intended to remain a long-term extension point.
4. Complete action 3 during the same pre-release API cleanup window.
5. Finish with action 5 so the docs reflect the final API shape.

## Completion Criteria

This action list is complete when:

- string-based slice access is no longer the primary supported consumer path
- the middleware contract reflects actual supported behaviors instead of exposing a larger store surface than needed
- the slice initial-state member is clearly named
- composition output remains understandable after any access redesign
- documentation matches the final reducer and slice contracts
