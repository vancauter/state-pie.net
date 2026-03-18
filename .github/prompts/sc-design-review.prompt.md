---
name: "sc-design-review"
description:
    "Use when reviewing C# and .NET code for design pattern fit, architecture boundaries, abstraction choices, and
    maintainability trade-offs."
argument-hint: "Scope, patterns to inspect, concerns to emphasize, or areas to exclude"
agent: "Design Review Expert"
---

# Design Review

Review the relevant C# and .NET code for design quality and pattern fit.

## Workflow

1. Read the relevant source, tests, and supporting docs before judging the design.
2. Evaluate the design with the shared review guidance and focus on the patterns, boundaries, or trade-offs relevant to
   this scope.
3. Distinguish immediate issues from future optional improvements.
4. Stay in review mode unless the user explicitly asks for implementation.

## Output expectations

1. Start with findings first, ordered by severity.
2. Separate concrete design flaws from acceptable trade-offs and future improvements.
3. Explain why each recommendation is worth the complexity it introduces.

## References

- [.NET design review guidance](../instructions/dotnet-design-review.instructions.md)
- [.NET architecture guidance](../instructions/dotnet-architecture.instructions.md)
