---
name: "commit"
description: "Autonomously generate and run git commits for the current workspace."
argument-hint: "Optional commit scope, grouping preference, or conventions to follow"
agent: "agent"
---

# Autonomous Commit

Autonomously generate and run git commits for the current workspace.

## Mission

- Work autonomously.
- Do not require user confirmation before creating commits.
- Use intelligent staging rather than staging everything by default.
- Create separate commits for separate logical changes.

## Commit Format

Use conventional commit formatting:

```text
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

## Workflow

1. Check git status.
    - Identify unstaged changes.
    - Identify whether staged changes already exist and need to be committed.
    - Use `git status --porcelain`.
2. Analyze the diff.
    - Prefer `git diff --staged` when staged changes exist.
    - Otherwise use `git diff`.
    - Determine the nature of each change, such as feature work, bug fixes, documentation updates, tests, or CI changes.
3. Group files into logical cohorts.
    - Use groups such as `docs`, `api`, `feature:<scope>`, `tests`, or `ci` when appropriate.
    - Create a separate staging set for each logical group.
    - Do not combine unrelated groups into the same commit.
    - Avoid staging files that appear to contain secrets, credentials, private keys, or environment files.
4. Generate commit messages.
    - Derive `type` from the change kind: `feat`, `fix`, `chore`, `docs`, `refactor`, `test`, or `ci`.
    - Derive `scope` from the affected paths when useful.
    - Use a short imperative description under 72 characters.
    - Add an optional body or footers such as `Closes #NNN` when applicable.
    - When multiple groups exist, generate a separate conventional commit message for each group.
5. Execute commits.
    - Stage only the files belonging to the current logical group.
    - Create one commit per group.
    - Prefer multi-line commits when the body or footers add value.
6. Repeat until the working tree is fully committed.
    - If additional unstaged changes remain after the first pass, repeat the process until all intended changes are
      staged and committed.

## Best Practices

- One logical change per commit.
- Use imperative present tense.
- Keep the summary line under 72 characters.
- Reference issues with `Closes #NNN` when applicable.

## Safety Rules

- Do not modify git config files.
- Do not run destructive commands such as force push or `git reset --hard` without explicit approval.
- Do not skip hooks with `--no-verify` unless explicitly requested.
- Do not stage unrelated changes together.
