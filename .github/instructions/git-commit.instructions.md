---
name: git-commit.instructions
description:
    Use when creating git commits, choosing Conventional Commit types and scopes, grouping staged changes, or writing
    commit messages, bodies, footers, and templates.
---

# Guidance

## Source of truth

- Before creating commits, inspect repository contribution guidance that might define branch, commit, sign-off, trailer,
  or review requirements.
- Check the relevant repository sources first when they exist: `README.md`, `CONTRIBUTING.md`, docs, issue templates,
  pull request templates, and existing commit history.
- If repository-specific guidance conflicts with generic guidance, follow the repository guidance.

## Normalized terminology

- Use `logical change` for the smallest coherent unit that should land together.
- Use `commit group` for one or more files or hunks staged together for a single commit.
- Use `type` for the Conventional Commit category such as `feat`, `fix`, or `docs`.
- Use `scope` for the optional noun that names the affected subsystem, package, or feature area.
- Use `summary` for the first line of the commit message.
- Use `body` for the optional explanatory paragraphs after the blank line.
- Use `footer` for structured trailer lines such as `BREAKING CHANGE:`, `Closes:`, or `Refs:`.

## Commit workflow

- Inspect `git status` and the relevant diff before staging or committing.
- Prefer one logical change per commit. If a diff mixes concerns, split it by file or hunk instead of forcing an
  umbrella commit.
- Stage only the files or hunks that belong to the current logical change.
- Prefer explicit staging over broad staging shortcuts when grouping matters.
- Never commit secrets, credentials, private keys, generated local-only artifacts, or environment-specific files unless
  the user explicitly intends that result and it is safe.
- Do not bypass hooks, rewrite published history, or use destructive git commands unless the user explicitly asks for
  it.

## Conventional Commit format

- Use the canonical structure:

```text
<type>[optional scope]: <summary>

[optional body]

[optional footer(s)]
```

- Use lowercase types for consistency.
- Use a scope only when it adds real signal. Skip empty or noisy scopes.
- Write the summary in imperative mood, focused on the resulting change, for example `add`, `fix`, `document`, `remove`.
- Keep the summary line short and scannable. Aim for roughly 50 to 72 characters.
- Avoid a trailing period in the summary line.
- Use the body for why, context, trade-offs, migration notes, or verification details when the summary alone is
  insufficient.
- Put footers after a blank line. Use them for structured metadata such as issue references, sign-offs, or
  breaking-change notes.

## Type selection

- `feat`: adds a user-facing or API-facing capability.
- `fix`: corrects a defect or broken behavior.
- `docs`: changes documentation only.
- `refactor`: restructures code without changing intended behavior.
- `perf`: improves performance characteristics.
- `test`: adds or updates tests.
- `build`: changes build tooling, packaging, or dependency management.
- `ci`: changes CI or automation workflows.
- `chore`: maintenance work that does not fit better elsewhere.
- `revert`: reverts an earlier change.
- `style`: formatting-only changes with no behavioral effect. Use sparingly.

## Breaking changes and footers

- Mark breaking changes with `!` before the colon, for example `feat(api)!:`.
- Add a `BREAKING CHANGE:` footer when the migration impact needs explicit explanation.
- Use repository-preferred issue references when known. Otherwise use conventional trailer lines such as `Closes:` or
  `Refs:`.
- Add `Signed-off-by:` only when the repository requires sign-offs.

## Commit template

```text
<type>[optional scope]: <imperative summary>

Why:
- <problem, motivation, or context>

What:
- <key change 1>
- <key change 2>

Validation:
- <test command or verification step>

[optional footer(s)]
```

## Quality checks

- Make sure the commit message matches the actual staged content, not the full working tree.
- Prefer multiple smaller commits when a single message would need `and` to describe unrelated work.
- If the commit is documentation-only, testing-only, or refactor-only, choose the more precise type instead of
  defaulting to `chore`.
- If the user asks for multiple commits, name each commit from its own staged diff rather than reusing a single broad
  description.
