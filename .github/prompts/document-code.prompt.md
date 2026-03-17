---
name: "document-code"
description: "Perform a comprehensive documentation and comment-refinement pass on the source code under ./src."
argument-hint: "Optional scope limits, comment style preferences, or areas to skip"
agent: "agent"
---

# Documentation Pass

Perform a comprehensive documentation and comment-refinement pass on all source code under `./src`. Comments explain the
**why**, not what the code does. Documentation should be clear, concise, and follow C# conventions.

## Tone and Perspective

Comments and documentation should be professional, direct, and focused on utility for developers. Avoid marketing
language or promotional content. Always write from a developer's perspective. Assume the reader is a competent developer
who needs to understand the intent, constraints, and non-obvious behavior of the code.

## Public API Documentation

- Apply full XML documentation to all public and protected members, including classes, methods, properties, and fields.
- Include `<summary>`, `<param>`, `<returns>`, and `<exception>` tags where applicable.
- Add `<exception cref="...">` tags for every exception that can be intentionally thrown by a method, and describe the
  exact triggering condition.
- Use `<remarks>` for non-obvious behavior, thread-safety guarantees, or usage constraints.
- Ensure every `<see cref="..."/>` tag correctly points to related types or methods.

## Internal Implementation Comments

- Provide XML `<summary>` documentation only for internal, private, and protected members.
- Add inline comments only for non-trivial logic or to explain why a decision exists.
- Omit comments for self-explanatory code such as simple assignments, standard loops, or basic instantiation.
- Explain intent rather than restating syntax.

## Quality Control

- Audit all existing comments.
- Remove or rewrite comments that are redundant, outdated, or poorly structured.
- Standardize all documentation text to professional C# conventions, including casing, punctuation, and phrasing.
- Verify that all XML tags match the current method signatures, parameter lists, return types, and generic type
  arguments.

## Quality Bar

- Aim for a clean-code aesthetic.
- Maximize documentation utility while minimizing visual noise.
