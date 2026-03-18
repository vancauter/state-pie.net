---
name: "sc-code-refactor"
description:
    "Use when refactoring C# code for naming, layout, architecture, visibility, type shape, or broader structural
    cleanup."
argument-hint: "Scope, preserve constraints, public APIs to keep, design concerns to emphasize, or areas to exclude"
agent: "C# Expert"
---

# Code Refactor

Run a focused or broad structural improvement pass over the relevant C# code.

## Workflow

1. Inspect the affected code before deciding on structural changes.
2. Review the code for naming, layout, architecture, visibility, type shape, and helper scope issues.
3. Inspect dependent tests, samples, docs, or tooling that need to move with the code.
4. Scale the pass to the repository.
5. Prefer the least risky change that materially improves the structure.
6. Present the plan before editing when the change is substantial.
7. Avoid cosmetic churn and avoid public-API changes unless the user explicitly asks for them.
8. Update tests and downstream consumers for renamed, moved, or structurally significant code.
9. Validate with the relevant build and test tasks.

## Output expectations

1. Start with a short assessment of the structural issues in scope.
2. Present the plan before editing when the pass is substantial.
3. Summarize exposure changes, type-shape changes, file moves, renames, and any intentional API or behavior changes.
4. Report validation results and remaining trade-offs.

## References

- [.NET architecture guidance](../instructions/dotnet-architecture.instructions.md)
- [.NET testing guidance](../instructions/dotnet-testing.instructions.md)
