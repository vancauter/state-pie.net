---
name: "dotnet-documentation"
description:
    "Use when performing a C# documentation pass to improve XML documentation comments, API explanations, and low-noise
    inline comments without changing behavior."
---

# .NET documentation guidance

- Prioritize public or reusable contracts before documenting internal helpers.
- Add XML documentation when it clarifies behavior, purpose, parameters, return values, invariants, nullability
  expectations, lifecycle rules, threading expectations, or meaningful exceptions.
- Keep `summary` text short and behavior-focused. Expand in `remarks` only when extra context materially helps the
  caller.
- Use `param`, `returns`, and `remarks` tags only when they add information that is not already obvious from the
  signature.
- Fix stale, vague, redundant, or misleading comments before adding new ones.
- Add inline comments only for intent, invariants, edge cases, lifecycle rules, or non-obvious trade-offs.
- Do not narrate syntax, repeat the implementation line by line, or restate names that are already self-explanatory.
- Recheck XML tag names, `cref` targets, and symbol references after editing.
- Keep terminology consistent with the repository's code, tests, and public surface.
- If documentation introduces or implies a behavioral guarantee, verify that the source and tests actually support it.
