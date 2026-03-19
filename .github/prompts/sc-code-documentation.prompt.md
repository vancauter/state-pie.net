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
3. Prioritize the contracts and comments that materially affect usability or maintainability in this scope.
4. Recheck signatures, XML tag names, and symbol references after editing.
5. If you encounter a code error or compile error, stop and report it instead of debugging it or making implementation
   changes. This prompt may update documentation only.

## Output expectations

1. Added, normalized, or improved XML documentation on public APIs.
2. Added, normalized, or improved inline comments.
3. Summarize the documentation scope when the pass is broad.
4. Report the documentation and commenting improvements after editing.
5. Call out any areas intentionally left undocumented or lightly documented.

## References

- [Documentation authoring guidance](../instructions/documentation-authoring.instructions.md)
- [.NET documentation guidance](../instructions/dotnet-documentation.instructions.md)
- [.NET architecture guidance](../instructions/dotnet-architecture.instructions.md)
