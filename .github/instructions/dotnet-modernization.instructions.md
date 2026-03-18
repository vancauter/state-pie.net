---
name: "dotnet-modernization"
description:
    "Use when cleaning up, modernizing, or performing janitorial maintenance on C# and .NET code, including warnings,
    obsolete APIs, dead code, and low-risk quality improvements."
---

# .NET modernization guidance

- Preserve behavior while reducing technical debt. Treat janitorial work as a quality pass, not a stealth redesign.
- Prefer small, verifiable improvements over broad churn.
- Remove dead or unused code only when its lack of use is clear from the current solution and public contract.
- Resolve warnings, obsolete APIs, and stale patterns when the modern replacement is stable and proportionate to the
  repository.
- Use newer C# or .NET features only when they improve clarity, safety, performance, or maintainability.
- Avoid mass syntax rewrites that make blame history noisier without improving the code materially.
- Keep modernization scoped. Separate cleanup from behavior changes, architectural refactors, or renaming campaigns
  whenever practical.
- Recheck tests, docs, and samples if the modernization changes observable usage or diagnostics.
- Validate incrementally when the pass touches multiple files.
