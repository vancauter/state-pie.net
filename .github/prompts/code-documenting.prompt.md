---
name: "document-code"
description:
    "Use when performing a comprehensive XML documentation and inline comment pass on C# source code under ./src."
argument-hint: "Optional scope limits, APIs to prioritize, comment density preferences, or areas to skip"
agent: "agent"
---

# Documentation Pass

Perform a comprehensive documentation and comment-refinement pass on all C# source code under `./src`.

Your goal is to produce useful, current, low-noise documentation:

- XML documentation explains the contract, usage, and constraints of APIs.
- Inline comments explain intent, rationale, invariants, or non-obvious behavior.
- Comments explain **why**, constraints, or implications, not what already-readable syntax does.
- Prefer no comment over a redundant comment.

Treat this as an implementation task, not a review-only task: update the source files directly.

## Mission

1. Audit existing XML documentation and inline comments.
2. Remove, rewrite, or tighten comments that are redundant, stale, vague, or grammatically weak.
3. Add missing XML documentation where the API surface or internal contract needs it.
4. Add inline comments only where they materially improve comprehension.
5. Keep the result professional, precise, and easy to scan.

## Tone and Perspective

Comments and documentation should be professional, direct, and focused on utility for developers. Avoid marketing
language or promotional content. Always write from a developer's perspective. Assume the reader is a competent developer
who needs to understand the intent, constraints, and non-obvious behavior of the code.

Write documentation as complete sentences with correct punctuation unless a tag convention clearly calls for a fragment.

## Public API Documentation

- Apply full XML documentation to all public and protected members, including classes, methods, properties, and fields.
- At a minimum, include `<summary>` on every public or protected type and member.
- Include `<param>`, `<typeparam>`, `<returns>`, and `<exception>` tags where applicable.
- Add `<exception cref="...">` tags for every exception that can be intentionally thrown by a method, and describe the
  exact triggering condition. Do not document incidental implementation exceptions that are not part of the intended
  contract.
- Use `<remarks>` only for supplemental information such as thread-safety guarantees, ordering rules, lifetime
  requirements, side effects, performance characteristics, or non-obvious usage constraints. Do not restate the summary
  in remarks.
- Use `<example>` with `<code language="csharp">` only when an example materially clarifies correct usage.
- Use `<value>` for public properties when the meaning of the represented value needs clarification beyond the summary.
- Ensure every `<see cref="..."/>`, `<seealso cref="..."/>`, and `<exception cref="...">` reference resolves to a valid
  symbol.
- Use `<paramref>` and `<typeparamref>` when referring to parameters or type parameters in prose.
- Use `<see href="...">` only for external URLs. Use `cref` for code elements.
- Use `<para>` when XML documentation needs multiple paragraphs or structured sections.
- Prefer `<inheritdoc/>` over duplicated XML comments when overrides or interface implementations inherit an unchanged
  contract.

## Internal Implementation Comments

- For internal, private, and protected implementation members, add XML `<summary>` only when the member exposes a
  reusable internal contract, enforces an invariant, or has intent that is not obvious from its signature and local
  context.
- Add inline comments only for non-trivial logic or to explain why a decision exists.
- Omit comments for self-explanatory code such as simple assignments, standard loops, or basic instantiation.
- Explain intent, invariants, edge cases, concurrency requirements, lifetime rules, security assumptions, or performance
  trade-offs rather than restating syntax.
- Prefer `//` comments on their own line. Avoid block comments (`/* */`) unless the surrounding file already uses them
  for a compelling reason.
- Do not add trailing end-of-line comments except in rare cases where a very short note is genuinely clearer inline.

## XML Documentation Rules

- Use `///` XML documentation comments, not plain triple-slash prose with no tags.
- Keep XML well-formed and consistent with the current signature.
- Match every `<param>` and `<typeparam>` name exactly to the declaration.
- Use `<c>` for inline code and `<code>` for multi-line code examples.
- Escape literal angle brackets in prose with `&lt;` and `&gt;` when needed.
- Avoid placeholder text such as `TODO`, `TBD`, `To be added.`, or empty tags.
- Do not document namespaces.

## Inline Comment Rules

- Comment only where a maintainer would reasonably ask "why is this here?" or "what constraint is this preserving?"
- Good inline comment targets include:
    - synchronization and locking decisions
    - caching or allocation-sensitive code
    - ordering dependencies
    - validation that encodes a business or protocol invariant
    - intentional fallbacks, compatibility workarounds, or defensive checks
    - subtle disposal, lifetime, or reentrancy behavior
- Bad inline comments include:
    - repeating the method name or signature
    - narrating assignments or branches that are already obvious
    - replacing expressive naming with commentary
    - large explanatory essays that belong in higher-level documentation
- Style inline comments consistently:
    - start with an uppercase letter
    - end with a period
    - leave one space after `//`
    - place the comment on its own line when possible

## Public vs Internal Balance

- Be comprehensive on externally consumed APIs.
- Be selective on internal implementation details.
- Favor strong summaries and targeted remarks over covering every line with commentary.
- If an internal helper becomes readable through naming alone after refactoring, prefer the clearer code and fewer
  comments.

## Quality Control

- Audit all existing comments.
- Remove or rewrite comments that are redundant, outdated, or poorly structured.
- Standardize all documentation text to professional C# conventions, including casing, punctuation, and phrasing.
- Verify that all XML tags match the current method signatures, parameter lists, return types, and generic type
  arguments.
- Verify that `<inheritdoc/>` remains accurate when used.
- Ensure that comments and docs align with actual runtime behavior, not aspirational behavior.
- Prefer Markdown links to existing workspace guidance when that avoids repeating policy text.

## Workflow

1. Read the relevant source files before editing.
2. Identify public API surface versus internal implementation details.
3. Fix or add XML documentation on the public/protected surface first.
4. Refine internal XML comments and inline comments only where they add value.
5. Remove noisy comments introduced only to satisfy blanket rules.
6. Validate signatures, XML tag names, and comment accuracy after editing.

## Decision Heuristics

When deciding whether to comment something, use these rules:

- If the code is already obvious from naming and structure, do not comment it.
- If a comment would only translate code into English, delete it or do not add it.
- If a reader needs context to avoid misuse, add XML docs.
- If a future maintainer could break behavior without noticing a subtle constraint, add an inline why-comment.
- If a member implements a documented contract unchanged, prefer `<inheritdoc/>`.
- If the API is simple and the summary already fully explains it, do not add decorative remarks.

## Output Expectations

- Edit the source files directly.
- Keep changes focused on documentation and comments unless a tiny supporting code cleanup is required to make the
  documentation truthful.
- If the requested scope is broad, work systematically across the affected files rather than partially documenting one
  area and stopping.

## Quality Bar

- Aim for a clean-code aesthetic.
- Maximize documentation utility while minimizing visual noise.
- Favor correctness and maintainability over comment count.
