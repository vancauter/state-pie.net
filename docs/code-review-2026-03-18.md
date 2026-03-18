# Code Review

Date: 2026-03-18

Scope: current AtomicState library codebase, with focus on the public store/state API, stream implementation, tests, and
README contract.

## Findings

### 1. `StateStream.Subscribe` accepts a null observer and degrades into a silent no-op instead of failing fast

Severity: medium

[StateStream](../src/AtomicState/Internal/StateStream.cs#L23) constructs a registration from `observer` without any null
check, and the callback path later routes through exception-swallowing notification helpers. With a null-oblivious
caller, `Subscribe(null!)` succeeds, adds a live registration, and later drops notifications by catching the resulting
`NullReferenceException` inside the notification path instead of rejecting the bad subscription at the boundary.

That is a poor fit for both normal `IObservable<T>` expectations and the rest of this library, which otherwise tries to
validate bad inputs at the boundary. It also makes the failure mode hard to diagnose because the caller gets neither an
immediate exception nor a visible stream failure.

Validation is also missing here: [StateStreamTests](../tests/AtomicState/StateStreamTests.cs#L175) cover disposal and
concurrency behavior, but there is no test asserting that `Subscribe(null!)` throws.

Why this is worth fixing:

- it restores a predictable public contract at a core API boundary;
- it turns a silent failure into an immediate, actionable exception;
- the fix is small and low-risk.

### 2. `StateAccess.GetSliceState` can throw `NullReferenceException` for `rootState == null`, which does not match its documented failure surface

Severity: medium

[StateAccess](../src/AtomicState/State/StateAccess.cs#L26) validates `sliceName` but not `rootState`. If a
null-oblivious caller passes `null!`, the method reaches `rootState.GetType().Name` in the error message path at
[StateAccess](../src/AtomicState/State/StateAccess.cs#L31) and throws `NullReferenceException`.

That is inconsistent with the nearby API shape because [StateAccess](../src/AtomicState/State/StateAccess.cs#L65)
already uses `ArgumentNullException.ThrowIfNull(rootState)` in `TryGetSliceState`. It also conflicts with the method's
own documented exception story, which describes invalid combined-state usage, missing slices, and bad casts, not a raw
null dereference.

The tests in [StateAccessTests](../tests/AtomicState/StateAccessTests.cs) cover unsupported state shapes, missing
slices, and wrong slice types, but not null input.

Why this is worth fixing:

- it makes the two slice-access APIs consistent with each other;
- it improves debuggability for consumers using null-oblivious code;
- it closes a public boundary bug without changing the core design.

### 3. Public factory methods rely on nullable annotations instead of runtime validation for required delegates and state instances

Severity: medium

[Factory.CreateSlice](../src/AtomicState/Store/Factory.cs#L64),
[Factory.CreateStore](../src/AtomicState/Store/Factory.cs#L85), and
[Factory.CreateStore with middleware](../src/AtomicState/Store/Factory.cs#L104) validate names and action types, but
they do not validate required runtime inputs such as `initialState`, `rootReducer`, or `reducer`.

In idiomatic nullable-aware C# that is usually fine, but this is a public library boundary. Null-oblivious callers,
reflection-based callers, or accidental `null!` use can still pass invalid values and get later
`NullReferenceException`s or invalid internal state instead of a clear argument exception at construction time.

The test suite reflects that gap: [FactoryTests](../tests/AtomicState/FactoryTests.cs#L33) and
[FactoryTests](../tests/AtomicState/FactoryTests.cs#L66) cover action-type and slice-name validation, but there is no
comparable coverage for null reducers or null initial state.

Why this is worth fixing:

- it makes the public API more robust across mixed nullable contexts;
- it keeps failure at the construction boundary instead of during later dispatch or slice access;
- it aligns the factory surface with the library's otherwise defensive argument validation.

## Action List

1. Add fast-fail null validation to `StateStream.Subscribe`. Files to update: `src/AtomicState/Internal/StateStream.cs`,
   `tests/AtomicState/StateStreamTests.cs`. Expected result: `Subscribe(null!)` throws `ArgumentNullException`
   immediately and the new test passes.
2. Add `rootState` null validation to `StateAccess.GetSliceState`. Files to update:
   `src/AtomicState/State/StateAccess.cs`, `tests/AtomicState/StateAccessTests.cs`. Expected result:
   `GetSliceState<TSlice>(null!, ...)` throws `ArgumentNullException` instead of `NullReferenceException`.
3. Add runtime argument validation to public factory entry points for required state and delegate inputs. Files to
   update: `src/AtomicState/Store/Factory.cs`, `tests/AtomicState/FactoryTests.cs`. Expected result: null-oblivious
   callers get clear `ArgumentNullException` failures for `initialState`, `rootReducer`, and `reducer`.
4. Run the non-performance test suite after the fixes land. Command:
   `dotnet test tests/*/*.Tests.csproj --filter "Category!=Perf"`. Expected result: the existing suite plus the new
   boundary-validation coverage passes cleanly.
5. Re-run a solution build to confirm no public-surface or nullable-regression fallout. Command: `dotnet build *.sln`.
   Expected result: clean build from the repository root.

## Suggested Order

- Do items 1 and 2 first because they are the smallest public-boundary fixes and close concrete consumer-facing defects.
- Do item 3 next because it touches more public entry points and will likely need the broadest test additions.
- Finish with items 4 and 5 as the review exit criteria.

## Positives

- The core architecture remains proportionate to the repository's size: `Factory`, `IStore`, composed reducers, and
  `StateAccess` form a small, coherent surface.
- The concurrency contract is documented clearly in [README](../README.md) and backed by focused stream and store tests.
- The recent stream-subscription and middleware-composition fixes improved the highest-risk lifecycle behavior without
  adding unnecessary abstraction layers.
- Validation coverage is generally strong for the main store and stream flows. The current non-performance test suite
  passed during this review with 60 passing tests.

## Residual Risks

- The main remaining release-readiness risk is inconsistent boundary validation across public APIs, not the core
  dispatch or stream model.
- Because this is a library, those input-validation gaps matter more than they would in an internal-only codebase:
  consumers will hit them from mixed nullable contexts that the compiler cannot fully police.

## Score

8.0/10

The design is compact, readable, and increasingly well-tested, and the recent lifecycle fixes materially improved the
most important behavior. The score stays below the high eights because the remaining public API boundary validation gaps
are concrete consumer-facing defects rather than optional polish.
