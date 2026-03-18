---
name: dotnet-testing
description:
    Use when writing, reviewing, organizing, or refactoring C# and xUnit tests in this repository, including naming,
    categories, ordering, fixtures, concurrency tests, and performance-test boundaries.
argument-hint: Describe the test target, scope, and whether this is unit, integration, or performance work.
---

# .NET Testing

Use this skill for test design and test implementation in this repository.

This skill sets the preferred convention for new and modified tests. If older tests differ in small stylistic ways,
follow this skill unless the existing file has a strong local pattern that would be made worse by mixing styles.

## Scope

- Framework: xUnit.
- Language: modern C# with nullable reference types enabled.
- Default test level: focused unit tests with no infrastructure dependency.
- Repository-specific split: performance tests are opt-in and live behind the `Category=Perf` trait.

## Classification Rules

Choose the smallest test type that proves the behavior.

- Unit test: Verifies one behavior in isolation, is deterministic, fast, and does not require real infrastructure.
- Integration test: Verifies a real boundary such as persistence, process integration, or external services. Use only
  when the boundary itself is the behavior under test.
- Performance test: Verifies a throughput, latency, or allocation budget. Keep these separate from the default test run.
- Acceptance or end-to-end test: Verifies an end-user flow across multiple layers. This repository does not currently
  use them, so add them only with explicit intent.

Prefer many unit tests, fewer integration tests, and a very small number of performance tests.

## Naming Rules

Follow the three-part structure recommended by current .NET guidance:

`MemberOrBehavior_WhenCondition_ExpectedOutcome`

Repository defaults:

- Start with the public member, operation, or observable behavior under test.
- Use `When...` for the scenario clause.
- End with the user-visible or contract-visible outcome.
- Keep names readable in Test Explorer. Prefer intent over abbreviations.
- Do not use `Test1`, `ShouldDoThing`, or names that only mirror the implementation.

Examples:

```csharp
[Fact]
public void CreateAction_WhenTypeIsBlank_ThrowsArgumentException()

[Fact]
public void Dispatch_WhenMiddlewareTriggersNestedDispatch_PreservesSerializedDispatchOrder()

[Theory]
[InlineData("")]
[InlineData(" ")]
public void CreateSlice_WhenNameIsBlank_ThrowsArgumentException(string name)
```

## Fact vs Theory

- Use `Fact` when a single scenario communicates the behavior clearly.
- Use `Theory` when the same behavior is exercised across multiple data rows.
- Convert duplicated one-line edge-case tests into a `Theory` when only inputs and expected values vary.
- Do not force a `Theory` when the data rows need different setup or different assertions.

## Test Body Structure

Use Arrange, Act, Assert even when you omit the comments.

- Keep one logical Act per test.
- If you need multiple Acts, the test must be proving a sequence, lifecycle, or cumulative behavior as the single
  behavior under test.
- Separate Arrange, Act, and Assert with blank lines.
- Extract local helpers only when they reduce duplication without hiding the behavior.

Preferred shape:

```csharp
[Fact]
public void Method_WhenCondition_ExpectedOutcome()
{
    var sut = CreateSut();

    var result = sut.Run();

    Assert.Equal(expected, result);
}
```

## Ordering and File Organization

Organize tests for scanability, not alphabetically by habit.

- Keep one test class per production type or behavior area.
- Keep file names aligned with the class under test, for example `StoreTests.cs`, `StateStreamTests.cs`, or
  `PerformanceTests.cs`.
- Inside a file, group tests by member or behavior area.
- Within each group, order from basic contract to guards, then edge cases, then concurrency or lifecycle cases.
- Keep private helper methods at the bottom of the file.
- Put shared test helpers under `tests/AtomicState/Shared/` only when they are reused by multiple test files.

Suggested in-file order:

1. Primary success-path behavior
2. Guard clauses and invalid input
3. Contract edge cases
4. Concurrency, re-entrancy, or disposal behavior
5. Local helper methods

## Category and Trait Rules

Only introduce traits that support a real execution policy.

- Do not tag ordinary unit tests with `Unit`. That is already the default expectation.
- Use `[Trait("Category", "Perf")]` for performance tests.
- If a future integration suite is added, use a stable exact value such as `Integration` and make sure the run command
  or task filters it intentionally.
- Keep trait values short, stable, and filter-friendly.

Repository default:

```csharp
[Trait("Category", "Perf")]
[Fact]
public void Dispatch_WhenReducerReturnsSameInstance_KeepsAllocationsNearZero()
```

## Fixtures and Shared Context

Prefer the narrowest lifetime that keeps tests independent.

- Constructor setup is the default in xUnit because each test gets a fresh class instance.
- Use `IDisposable` or `IAsyncLifetime` only when cleanup is required.
- Use `IClassFixture<T>` only for expensive setup that is safe to share within one class.
- Use collections only when multiple classes must share expensive state or must be isolated from parallel execution
  together.
- Avoid hidden ambient setup that makes a test hard to read.

In this repository, lightweight hand-rolled helpers are preferred over broad fixture hierarchies.

## Parallelism and Ordering Constraints

Assume tests may run in parallel unless the framework configuration or collection membership says otherwise.

- Tests must not depend on execution order.
- Do not rely on alphabetical ordering, source ordering, or Test Explorer presentation.
- If ordering becomes unavoidable, document why and also disable the relevant parallel execution path.
- Shared mutable state across test classes requires explicit collection design.

## Concurrency Test Rules

Concurrency tests are allowed here, but they must be explicit and bounded.

- Use deterministic coordination primitives such as `Barrier`, `TaskCompletionSource`, `Interlocked`, `Volatile`, and
  bounded `SpinWait` loops.
- Every wait must have a timeout.
- Prefer proving an observable contract, not a scheduler accident.
- Add a short inline comment when a synchronization step is non-obvious.

Example comment style for non-obvious coordination:

```csharp
// Advance the committed marker after dispatch so the reader only acknowledges globally visible states.
Volatile.Write(ref committedTarget, expected);
```

## Assertions and Data Choices

- Assert the narrowest contract that matters.
- Capture an exception into a variable only when you also need to assert message text or additional properties.
- Use minimal passing data unless a specific value is part of the behavior.
- Replace opaque literals with named constants when the literal does not explain itself.
- Avoid loops and branching inside tests unless the loop is the subject of the test or the clearest way to express a
  bounded concurrency probe.

## Test Doubles and Helpers

Use the correct testing vocabulary and the lightest practical tool.

- Fake: working lightweight implementation.
- Stub: provides canned answers.
- Spy: records calls or values for later assertions.
- Mock: verifies expected interactions.

Repository preference:

- Prefer simple hand-written observers, reducers, slices, and callbacks in `tests/*/Shared/` over a mocking framework.
- Reach for interaction-style verification only when the interaction order or callback behavior is itself the contract.

## Output and Diagnostics

- Prefer assertions over diagnostic output.
- For ordinary xUnit v2 diagnostics, use `ITestOutputHelper` when output is needed during debugging.
- Testing `Console` output directly is acceptable only when console writing is the real behavior under test.

## Repository Conventions That Fill In the Gaps

Use the existing suite as the tie-breaker where public guidance is silent.

- Namespace tests under `SC.*.Tests`.
- Keep reusable helpers in `tests/*/Shared/`.
- Keep performance coverage in `PerformanceTests.cs` and gate it with `Category=Perf`.
- Prefer local static helpers at the bottom of the file for complex concurrency setup.
- Add comments only where the synchronization, performance warmup, or memory behavior would otherwise be hard to infer.

## Workflow

When adding or revising tests, follow this order:

1. Classify the test: unit, integration, performance, or acceptance.
2. Pick `Fact` or `Theory` based on whether only the data varies.
3. Name the test with `MemberOrBehavior_WhenCondition_ExpectedOutcome`.
4. Keep setup explicit and local unless a shared helper materially improves clarity.
5. Keep the test deterministic and independent from run order.
6. Add a trait only if it changes how the suite is filtered or run.
7. Add brief inline comments only for non-obvious coordination or measurement setup.

## Review Checklist

Before finishing a test change, verify all of the following:

- The test name explains the behavior without reading the production code.
- The test level is the smallest one that proves the contract.
- The test can run independently and in parallel unless explicitly documented otherwise.
- Data and assertions are minimal but sufficient.
- Categories and traits match an actual execution policy.
- Any comments explain non-obvious intent rather than restating the code.
