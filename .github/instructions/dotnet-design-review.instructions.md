---
name: "dotnet-design-review"
description:
    "Use when reviewing C# or .NET code for pattern fit, architecture boundaries, abstraction shape, extension surface,
    and maintainability risks without implementing changes."
---

# .NET design review guidance

- Review the design that exists before recommending a different pattern. Prefer better fit over pattern churn.
- Focus on responsibility boundaries, dependency direction, extension points, lifecycle rules, and public-surface
  clarity.
- Check whether abstractions earn their cost.
    - Flag interfaces or layers that add ceremony without substitution value.
    - Flag overly concrete code only when it blocks legitimate extension, testing, or composition.
- Review common .NET pattern usage pragmatically.
    - Construction and composition: factories, dependency injection seams, object ownership.
    - Behavioral flows: middleware, pipelines, reducers, commands, event dispatch, callbacks.
    - Access patterns: repositories, providers, adapters, or gateways when those boundaries exist.
- Look for violations that increase maintenance risk: mixed responsibilities, hidden coupling, inconsistent async
  boundaries, unclear ownership, or accidental inheritance surface.
- Review naming and discoverability as part of the design. A sound architecture with opaque names is still costly to
  use.
- Consider testability as a design signal, not as a reason to add abstractions mechanically.
- Prefer review feedback that preserves the repository's current scale. Do not prescribe enterprise patterns to solve
  small-library problems.
- When a pattern is missing, explain the concrete pain it would solve before recommending it.
- Distinguish clearly between:
    - design flaws that should be fixed now
    - trade-offs that are acceptable for the current repository size
    - future-facing improvements that are optional
