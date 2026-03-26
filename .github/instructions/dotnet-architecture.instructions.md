---
name: "dotnet-architecture"
description:
    "Use when working on C# or .NET project files and you need architecture, design, and implementation guidance
    proportionate to the codebase."
applyTo: "**/*.cs,**/*.csproj,**/Program.cs"
---

# .NET architecture guidance

- Match the architecture to the size and complexity of the codebase.
    - Small libraries or services should stay simple and avoid ceremonial layers.
    - Medium solutions should separate responsibilities when that clarifies dependencies, ownership, or deployment
      boundaries.
    - Larger systems should make module boundaries, contracts, and ownership explicit.
- Keep domain language consistent across types, namespaces, tests, and documentation.
- Prefer changes that solve the root cause without expanding the abstraction surface unnecessarily.
- Preserve public contracts unless the task explicitly calls for a breaking change.
- Prefer composition over inheritance unless substitution is a real part of the design.
- Use modern C# and .NET features when they improve clarity, correctness, or safety, not just because they are
  available.
- Keep async code end to end for I/O-bound work. Avoid blocking on tasks or mixing sync and async flows without a clear
  reason.
- Validate inputs at the boundary where invalid data enters the system. Keep business rules near the behavior they
  protect.
- When editing project files, keep dependencies deliberate, package additions justified, and target-framework changes
  explicit.
- Measure before applying hot-path optimizations, and document the constraint you are optimizing for when it is not
  obvious.
- Add or update tests when behavior changes, and run the narrowest relevant validation before finishing.
