---
name: "design-hygiene"
description: "Run a focused C# design pass for least exposure, type shape, and adjacent maintainability improvements."
argument-hint: "Optional scope, folders, or extra design constraints to emphasize"
agent: "C# Expert"
---

# Design Hygiene Pass

Perform a focused design-quality pass over the current C# codebase. The goal is to identify and fix issues related to
design hygiene, including excessive exposure, suboptimal type shapes, while minimizing churn.

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

3. Type and member restrictions.
    - Check for opportunities to use `sealed`, `static`, `readonly`, or file-scoped and internal-only helpers when they
      better express intent.
    - Remove inheritance surface that is not actually intended for extension.

4. Validation.
    - Add or update tests for any behavioral or structural change that needs protection.
    - Prefer targeted tests over broad churn.
    - Build and run relevant tests when possible.

## Workflow

1. Read applicable repository instructions and any relevant C#-specific guidance first.
2. Inspect the affected code before proposing changes.
3. Group findings into:
    - exposure issues
    - type-shape issues
    - related design-hygiene issues
4. Make only changes that are justified and low-risk.
5. Preserve naming and architecture conventions already established in the repository unless there is a clear
   inconsistency.
6. Avoid cosmetic churn.

## Decision Rules

When deciding whether to change code, use these heuristics:

- Prefer lower visibility if no real caller needs the broader one.
- Prefer `record` for immutable data-centric types with value semantics.
- Prefer `struct` or `record struct` only when value semantics are clearly desired and copying costs remain reasonable.
- Prefer `class` for reference identity, mutable state, inheritance-based design, or larger objects.
- Prefer `static` methods when no instance state is required.
- Prefer leaving code alone when the "improvement" is subjective, risky, or public-API-affecting.

## Constraints

- Do not introduce new abstractions unless they clearly improve maintainability or testability.
- Do not rewrite large areas of code for stylistic consistency alone.

## Output Expectations

1. Start with a short assessment of the current code quality in this pass's scope.
2. Present the proposed changes before editing when the pass is substantial.
3. After implementation, summarize:
    - exposure changes
    - type-shape changes
    - additional design-hygiene improvements
4. Report build and test results.
5. Call out any issues intentionally left unchanged because they would require public API changes or disproportionate
   churn.
