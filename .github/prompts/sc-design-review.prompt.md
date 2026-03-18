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
5. If you encounter a code error or compile error, stop and report it. Do not debug it or make implementation changes in
   this prompt.

## Output expectations

1. Code review and actions:
    - ./docs/internal/[yyyyMMdd]-[##]-design-review.md (The review, conclusions, and recommendations)
    - ./docs/internal/[yyyyMMdd]-[##]-design-review-actions.md (Action checklist based on the review)
2. Start with findings first, ordered by severity.
3. Separate concrete design flaws from acceptable trade-offs and future improvements.
4. Explain why each recommendation is worth the complexity it introduces.

## References

- [.NET design review guidance](../instructions/dotnet-design-review.instructions.md)
- [.NET architecture guidance](../instructions/dotnet-architecture.instructions.md)
