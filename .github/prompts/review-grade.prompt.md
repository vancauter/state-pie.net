---
name: "review-grade"
description: "Review the current project, list positives and negatives, and assign a 1-10 grade with one decimal based on the project's intended scope."
argument-hint: "Optional focus area or scope emphasis"
agent: "agent"
---

Review the entire project in the current workspace.

If the user provides an argument, treat it as an additional focus area while still reviewing the whole project.

Use this workflow:

1. Read the repository instructions and the main project documentation before judging quality.
2. Inspect the core source files, test files, and any release-readiness or operational docs that materially affect project quality.
3. Validate the current state with the existing build and non-performance test commands or tasks when available.
4. Judge the project relative to its intended scope. If the project is deliberately small or simple, do not penalize it for missing enterprise-scale features unless those omissions create concrete risks inside the chosen scope.

Output requirements:

1. Start with findings first, ordered by severity, with concrete file references when applicable.
2. Then provide a short assumptions section if any assumptions materially affect the review.
3. Then provide positives.
4. Then provide negatives.
5. End with a numeric grade in the format `x.y/10` and explain the reasoning for that score.

Review standards:

- Prioritize bugs, behavioral risks, maintainability issues, API rough edges, missing validation, missing tests, and release-readiness gaps.
- Keep the summary concise and evidence-based.
- Explicitly say when the build or tests were not run.
- If no meaningful findings are discovered, state that clearly and focus on residual risks and testing gaps.
