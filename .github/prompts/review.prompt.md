---
name: "review"
description:
    "Review the current project, list positives and negatives, and assign a 1-10 grade with one decimal based on the
    project's intended scope."
argument-hint: "Optional focus area or scope emphasis"
agent: "agent"
---

# Project Review

Review the entire project in the current workspace.

If the user provides an argument, treat it as an additional focus area while still reviewing the whole project.

## Workflow

1. Read the repository instructions and main project documentation before judging quality.
2. Inspect the core source files, test files, and any release-readiness or operational docs that materially affect
   project quality.
3. Validate the current state with the existing build and non-performance test commands or tasks when available.
4. Judge the project relative to its intended scope. If the project is deliberately small or simple, do not penalize it
   for missing enterprise-scale features unless those omissions create concrete risks inside the chosen scope.

## Review Standards

- Prioritize bugs, behavioral risks, maintainability issues, API rough edges, missing validation, missing tests, and
  release-readiness gaps.
- Keep the summary concise and evidence-based.
- Explicitly say when the build or tests were not run.
- If no meaningful findings are discovered, state that clearly and focus on residual risks and testing gaps.

## Output Expectations

1. Start with findings first, ordered by severity, with concrete file references when applicable.
2. Provide a short assumptions section if any assumptions materially affect the review.
3. Provide positives.
4. Provide negatives.
5. End with a numeric grade in the format `x.y/10` and explain the reasoning for that score.
