---
name: "sc-code-review"
description: "Use when reviewing the current codebase and reporting findings relative to its intended scope."
argument-hint: "Focus area, risk category, or scope emphasis"
agent: "C# Expert"
---

# Code Review

Review the current codebase and report the findings.

## Workflow

1. Inspect the relevant source, tests, and supporting docs (inc. past reviews) before judging quality.
2. Run the narrowest relevant build or test validation when it materially strengthens the review.
3. Judge the code relative to the repository's intended scope. Do not punish a deliberately small codebase for not being
   an enterprise platform.
4. Prioritize concrete bugs, behavioral risks, maintainability problems, weak validation, missing tests, and
   release-readiness gaps.

## Output expectations

1. Code review and actions:
    - ./docs/internal/[yyyyMMdd]-[##]-code-review.md (The review, conclusions, and recommendations)
    - ./docs/internal/[yyyyMMdd]-[##]-code-review-actions.md (Action checklist based on the review)
2. Start with findings first, ordered by severity, with concrete file references where applicable.
3. Provide assumptions only when they materially affect the review.
4. Summarize positives and residual risks.
5. End with a numeric grade in the format `x.y/10` and explain the score briefly.

## References

- [.NET architecture guidance](../instructions/dotnet-architecture.instructions.md)
- [.NET testing guidance](../instructions/dotnet-testing.instructions.md)
