---
name: "sc-code-documentation"
description: "Use when performing a C# documentation pass to improve XML docs and low-noise inline comments."
argument-hint: "Scope, APIs to prioritize, comment density preference, or areas to skip"
agent: "Documentation Expert"
---

# Code Documentation

Improve XML documentation and inline comments for the relevant C# code.

## Workflow

1. Read the affected source files before editing.
2. Read the relevant callers or tests when they define the real contract or usage expectations.
3. Match the pass to the codebase and request.
    - Small or internal-only areas: prioritize public surface and genuinely non-obvious internals.
    - Larger or externally consumed areas: document contracts, constraints, invariants, and usage expectations more
      comprehensively.
4. Fix stale, vague, redundant, or misleading comments before adding new ones.
5. Add XML documentation where it clarifies a public or reusable contract.
6. Add inline comments only for intent, invariants, edge cases, lifecycle rules, or non-obvious trade-offs.
7. Recheck signatures, XML tag names, and symbol references after editing.

## Quality bar

- Favor self-explanatory code over comment volume.
- Keep comments precise, current, and professional.
- Do not narrate syntax that is already obvious from the code.

## References

- [Documentation authoring guidance](../instructions/documentation-authoring.instructions.md)
- [.NET documentation guidance](../instructions/dotnet-documentation.instructions.md)
- [.NET architecture guidance](../instructions/dotnet-architecture.instructions.md)
