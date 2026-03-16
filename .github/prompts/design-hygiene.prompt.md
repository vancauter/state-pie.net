---
name: "design-hygiene"
description:
    "Run a focused C# design pass for least exposure, type shape, purity, and adjacent maintainability improvements
    without changing the public API."
argument-hint: "Optional scope, folders, or extra design constraints to emphasize"
agent: "C# Expert"
---

# Design Hygiene Pass

Perform a focused design-quality pass over the current C# codebase.

Consult [DDD and .NET architecture guidelines](../instructions/dotnet-architecture.instructions.md) when the reviewed
files fall under that instruction scope.

## Mission

Review the implementation for:

1. Least exposure.
    - Prefer `private` over `internal`, `internal` over `protected`, and `protected` over `public`.
    - Flag members and types that are more visible than necessary.
    - Reduce visibility only when it does not change the public contract or break legitimate internal consumers.

2. Best-fitting type shape.
    - Check whether each type is best represented as a `class`, `record`, `struct`, `record struct`, or `static` class.
    - Prefer the type that best matches identity, mutability, allocation, and value-semantics needs.
    - Do not rewrite public APIs in ways that break consumers unless explicitly requested.

3. Pure-function opportunities.
    - Prefer pure functions where practical.
    - Identify methods that can become static, side-effect-free, or easier to reason about.
    - Improve purity inside the implementation only.
    - Do not change the public interface shape unless explicitly requested.

## Additional Review Requirements

Also check for the following because they frequently surface during the same pass:

4. Immutability and intent.
    - Prefer immutable state and immutable data carriers where appropriate.
    - Check whether fields, properties, and helper types communicate mutability clearly.
    - Prefer `readonly` where it strengthens correctness without harming usability.

5. Type and member restrictions.
    - Check for opportunities to use `sealed`, `static`, `readonly`, or file-scoped and internal-only helpers when they
      better express intent.
    - Remove inheritance surface that is not actually intended for extension.

6. Responsibility and cohesion.
    - Check whether classes have a single clear reason to change.
    - Extract helpers only when it materially improves cohesion, purity, or readability.
    - Avoid abstraction for abstraction's sake.

7. Public API preservation.
    - Preserve public behavior and public surface unless the task input explicitly allows API changes.
    - Prefer internal refactors over consumer-facing changes.

8. Validation.
    - Add or update tests for any behavioral or structural change that needs protection.
    - Prefer targeted tests over broad churn.
    - Build and run relevant tests when possible.

## Workflow

1. Read applicable repository instructions and any relevant C#-specific guidance first.
2. Inspect the affected code before proposing changes.
3. Group findings into:
    - exposure issues
    - type-shape issues
    - purity issues
    - related design-hygiene issues
4. Make only changes that are justified and low-risk.
5. Preserve naming and architecture conventions already established in the repository unless there is a clear
   inconsistency.
6. Avoid cosmetic churn and avoid changing code only to satisfy style preferences without a meaningful design benefit.

## Decision Rules

When deciding whether to change code, use these heuristics:

- Prefer lower visibility if no real caller needs the broader one.
- Prefer `record` for immutable data-centric types with value semantics.
- Prefer `struct` or `record struct` only when value semantics are clearly desired and copying costs remain reasonable.
- Prefer `class` for reference identity, mutable state, inheritance-based design, or larger objects.
- Prefer `static` methods when no instance state is required.
- Prefer pure helpers when side effects are unnecessary.
- Prefer leaving code alone when the "improvement" is subjective, risky, or public-API-affecting.

## Constraints

- Do not change public interfaces, public type names, or public member signatures unless explicitly requested.
- Do not introduce new abstractions unless they clearly improve maintainability or testability.
- Do not perform broad renames or file moves unless they are necessary for a justified design correction.
- Do not rewrite large areas of code for stylistic consistency alone.

## Output Expectations

1. Start with a short assessment of the current code quality in this pass's scope.
2. Present the proposed changes before editing when the pass is substantial.
3. After implementation, summarize:
    - exposure changes
    - type-shape changes
    - purity improvements
    - additional design-hygiene improvements
4. Report build and test results.
5. Call out any issues intentionally left unchanged because they would require public API changes or disproportionate
   churn.
