# AtomicState Restructure Review

## Scope

This document captures the full-source restructure review performed against the current repository state.

Reviewed areas:

- `src/AtomicState`
- `tests/AtomicState`
- `dev/AtomicState`
- `README.md`
- `docs/TODO.md`

Validation baseline:

- `dotnet build *.sln`
- `dotnet test tests/*/*.Tests.csproj --filter "Category!=Perf"`

Current baseline status:

- Build passes.
- Non-performance tests pass.

## Step 1: Current Architecture Assessment

### What is working well

- The public API is intentionally small and coherent.
- Core concepts are clear: actions, reducers, slices, store, middleware, and state observation.
- The store is the clear consistency boundary for dispatch ordering and state publication.
- The custom replaying state stream is well tested, including race scenarios.

### What is structurally weak

- The folder layout is implementation-oriented instead of concept-oriented.
- The codebase is small, but the current file and folder split makes it feel larger and more layered than it is.
- Combined slice access is missing from the public API, which forces tests to use reflection.
- Store initialization failure behavior is implicit and currently suppresses all initialization exceptions.
- The development example is more fragmented than necessary for its size.

## Step 2: DDD and SOLID Review

### Domain concepts and boundaries

- The main domain concepts in this library are `IStore`, `IState`, `IAction`, `ISlice`, reducers, and middleware.
- The store is the aggregate-like boundary because it owns serialized dispatch, committed state, and lifecycle
  transitions.
- Combined state is an internal composition detail, but consumers still need a supported way to read slice values.

### Layer impact

- Public/domain-facing API layer:
    - `IAction`
    - `IState`
    - `ISlice`
    - `IStore`
    - reducer and dispatch delegates
    - `Factory`
- Infrastructure/runtime layer:
    - `Store`
    - `CombinedState`
    - `StateSlice<TState>`
    - `StoreAction`
    - `StateStream`
    - `LoggingMiddleware`

### SOLID assessment

- SRP: mostly good, but `Store` currently owns dispatch serialization, initialization policy, and disposal policy.
- OCP: acceptable for the current library scope.
- LSP: no obvious substitution problems.
- ISP: the public interfaces are narrow and appropriate.
- DIP: acceptable for a small library; additional abstractions would likely be unnecessary.

### Ubiquitous language assessment

- Type names are consistent with the library domain.
- Folder names are less consistent with the domain language because they emphasize mechanics such as `Abstractions` and
  `Runtime`.

### Security and compliance considerations

- This repository is not modeling a financial domain, so PCI-DSS, SOX, LGPD, and decimal monetary rules are not directly
  applicable.
- The relevant reliability concern is hidden failure: initialization exceptions are currently swallowed, which weakens
  diagnosability.

### Domain events

- No domain events are warranted here.
- This library is infrastructure-oriented and does not model business-significant domain occurrences.

## Step 3: Structural Findings Grouped By Concern

### Structure

#### Finding

The layout is too segmented for the size of the library.

Current structure:

- `Abstractions`
- `Factory`
- `Middleware`
- `Runtime`

Why this is a problem:

- It describes implementation categories rather than the consumer-facing concepts of the library.
- The partial `Factory` split across multiple files adds navigation cost without reducing complexity.
- The `StateStream` partial split across multiple files is only marginally helpful at the current size.

Recommended change:

- Flatten the public API files into concept-oriented areas or a simpler root layout.
- Rename `Runtime` to `Internal` if the intent is to mark implementation detail.
- Merge the four `Factory` partial files into a single `Factory.cs`.
- Merge the `StateStream` partial files into one file, or at most keep one companion file for queueing internals.

Expected outcome:

- Easier navigation.
- Less structural ceremony.
- A project shape that better matches the actual scope of the library.

### Naming

#### Finding

Type names are generally good, but folder names and a few internal concepts signal implementation rather than intent.

Why this is a problem:

- Small libraries benefit from obvious structure more than from taxonomy depth.
- Consumers think in terms of store, state, slices, and actions, not abstraction buckets.

Recommended change:

- Preserve current public type names unless a deliberate breaking API cleanup is desired.
- Improve naming primarily through folder and file layout.
- Prefer domain-oriented organization over technical grouping.

Expected outcome:

- Stronger alignment between project structure and ubiquitous language.

### Public API Gaps

#### Finding

Combined slice state has no supported public accessor.

Evidence:

- Tests use reflection in `StoreStateAssertions` to read internal combined-state data.
- README guidance currently points users toward serialization or observation rather than a proper accessor.

Why this is a problem:

- Reflection in tests is a strong signal that a public contract is missing.
- Combined reducers are already a supported public feature, so reading combined state should also be a supported
  scenario.

Recommended change:

- Add a public combined-state access contract.
- Prefer a typed, non-throwing lookup such as `TryGet` by slice name and expected type.
- Keep the store surface small by retaining `IState` as the root state contract.

Expected outcome:

- Tests can stop reflecting over internal runtime types.
- Consumers gain a supported way to read slice state.
- Documentation can stop recommending workarounds.

### Runtime Behavior

#### Finding

Store initialization currently suppresses all exceptions from the internal `@@INIT` dispatch.

Why this is a problem:

- A caller may receive a store instance even though initialization logic failed.
- That makes failure modes harder to detect and reason about.
- The current behavior is already tracked as an open concern in `docs/TODO.md`.

Recommended change:

- Remove the blanket exception suppression from the initialization path.
- Make initialization fail store creation by default.
- If needed later, add an explicit options-based policy for softer handling rather than hiding the behavior.

Expected outcome:

- Clearer failure contract.
- Better diagnosability.
- Less surprising runtime behavior.

### Test Structure

#### Finding

The tests are strong behaviorally, but some structure mirrors internal implementation details rather than public
responsibilities.

Why this matters:

- Public behavior tests should stay focused on API contracts.
- Internal tests should exist only where internal behavior is intentionally exposed through `InternalsVisibleTo`.

Recommended change:

- Keep store, middleware, and factory tests centered on public behavior.
- Retain direct `StateStream` tests as runtime-internal verification if that internal complexity remains.
- Replace reflection-based combined-state assertions with tests against the new public accessor.
- Continue using the `MethodName_Condition_ExpectedResult()` naming pattern.

Expected outcome:

- Cleaner separation between public API verification and internal implementation verification.

### Dev Example and Documentation

#### Finding

The dev sample is slightly oversplit, and the README describes some placeholder or workaround behaviors as if they were
acceptable steady-state guidance.

Why this matters:

- The sample app should demonstrate intended usage patterns.
- The README should reflect the actual shipping reality and supported API.

Recommended change:

- Merge `ExampleSlices` and `ExampleStoreFactory` into a simpler example store file.
- Update the sample program to demonstrate supported combined-state access once added.
- Replace placeholder package installation guidance with the actual distribution story.
- Document the explicit store initialization failure contract.

Expected outcome:

- More credible docs.
- Better consumer guidance.
- Less mismatch between implementation and documentation.

## Step 4: Proposed Restructure Plan

### Priority 1: Fix the highest-value API and behavior gaps

1. Add a supported combined-state accessor API.
2. Make store initialization failure explicit and fail-fast by default.
3. Update tests and docs to use the new contract.

Why this goes first:

- These are real design issues, not just cosmetic structure concerns.
- They remove reflection and hidden failure behavior.

### Priority 2: Simplify the source layout

1. Collapse the `Factory` partial split into one file.
2. Rename `Runtime` to `Internal` if preserving an implementation-detail boundary is useful.
3. Flatten or simplify the public API folder layout.
4. Collapse the `StateStream` partial split if it no longer improves clarity.

Why this goes second:

- These changes improve navigation and maintainability but do not fix the most important API issues on their own.

### Priority 3: Align dependent projects

1. Update tests to use the supported combined-state API.
2. Simplify the dev example structure.
3. Update README and `docs/TODO.md` to reflect the revised contracts.

Why this goes third:

- These changes depend on the public API and runtime decisions above.

## Step 5: Files Most Likely To Change In A Real Restructure Pass

### Core library

- `src/AtomicState/Factory/CombineReducers.cs`
- `src/AtomicState/Factory/CreateAction.cs`
- `src/AtomicState/Factory/CreateSlice.cs`
- `src/AtomicState/Factory/CreateStore.cs`
- `src/AtomicState/Runtime/CombinedState.cs`
- `src/AtomicState/Runtime/Store.cs`
- `src/AtomicState/Runtime/StateStream/StateStream.cs`
- `src/AtomicState/Runtime/StateStream/StateStream.Notification.cs`
- `src/AtomicState/Runtime/StateStream/StateStream.Registration.cs`
- `src/AtomicState/Runtime/StateStream/StateStream.Subscription.cs`
- `src/AtomicState/Abstractions/Dispatch.cs`
- `src/AtomicState/Abstractions/IAction.cs`
- `src/AtomicState/Abstractions/ISlice.cs`
- `src/AtomicState/Abstractions/IState.cs`
- `src/AtomicState/Abstractions/IStore.cs`
- `src/AtomicState/Abstractions/Reducer.cs`

### Tests

- `tests/AtomicState/StoreTests.cs`
- `tests/AtomicState/FactoryTests.cs`
- `tests/AtomicState/Shared/StoreStateAssertions.cs`
- `tests/AtomicState/ReplayStateStreamTests.cs`

### Dev and docs

- `dev/AtomicState/ExampleStore/ExampleSlices.cs`
- `dev/AtomicState/ExampleStore/ExampleStoreFactory.cs`
- `dev/AtomicState/Program.cs`
- `README.md`
- `docs/TODO.md`

## Step 6: Test Changes That Should Accompany The Restructure

Recommended additions or updates:

- `CreateStore_WhenInitializationReducerThrows_ThrowsInvalidOperationException()`
- `CreateStore_WhenInitializationMiddlewareThrows_ThrowsInvalidOperationException()`
- `CombinedState_WhenSliceExists_ReturnsTypedSliceState()`
- `CombinedState_WhenSliceMissing_ReturnsFalse()`
- `CombinedState_WhenSliceTypeMismatches_ReturnsFalse()`
- `CombineReducers_WhenNoSliceChanges_ReturnsExistingCombinedState()`

These should remain focused on behavior rather than internal implementation shape.

## Step 7: Changes Not Recommended

- Do not add more interfaces solely for architectural purity.
- Do not introduce domain events.
- Do not add application-service layering to this library.
- Do not over-engineer around extensibility that the project does not currently need.

## Step 8: Final Recommendation

If the restructure were executed now, the recommended order would be:

1. Add the public combined-state access API.
2. Make initialization failures explicit and fail-fast.
3. Update tests and documentation to match those contracts.
4. Simplify `Factory` and `StateStream` file structure.
5. Simplify the folder layout to better match the library's domain language.
6. Simplify the dev example to demonstrate the intended public API.

This sequence fixes the root design issues first and leaves layout cleanup as a reinforcing step rather than the main
event.
