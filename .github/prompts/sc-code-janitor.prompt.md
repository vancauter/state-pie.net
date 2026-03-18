---
name: "sc-code-janitor"
description:
    "Use when cleaning up, modernizing, or performing a janitorial maintenance pass on C# and .NET code without broad
    redesign."
argument-hint: "Scope, warnings to target, modernization constraints, or areas to leave untouched"
agent: "C# Expert"
---

# Code Janitor

Run a focused cleanup and modernization pass over the relevant C# and .NET code.

## Workflow

1. Inspect the affected code and identify cleanup work that can be done without broad redesign.
2. Prioritize warnings, obsolete APIs, dead code, low-risk modernization, and small maintainability improvements.
3. Keep cleanup separate from larger structural refactors whenever practical.
4. Validate incrementally when the pass affects multiple files.
5. Preserve behavior unless the user explicitly asks for a broader change.

## Output expectations

1. Summarize the cleanup categories in scope before large passes.
2. Report the major modernization or janitorial changes after editing.
3. Report validation results and any intentionally deferred cleanup.

## References

- [.NET modernization guidance](../instructions/dotnet-modernization.instructions.md)
- [.NET architecture guidance](../instructions/dotnet-architecture.instructions.md)
- [.NET testing guidance](../instructions/dotnet-testing.instructions.md)
