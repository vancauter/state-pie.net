---
name: "sc-git-commit"
description: "Use when creating one or more git commits for the current workspace."
argument-hint: "Commit scope, grouping preference, or message conventions to follow"
agent: "Git Commit Expert"
---

# Git Commit

Create one or more git commits for the current workspace.

## Workflow

1. Inspect git status and diff before staging anything.
2. Group files by logical change instead of committing everything together by default.
3. Stage only the files that belong to the current logical group.
4. Name each commit from the staged diff, not from the entire working tree.
5. Use Conventional Commit formatting with a short imperative summary and add a body or footer when the change needs
   context.
6. Repeat until all intended changes are committed.

## Output expectations

1. Summarize the logical commit groups.
2. Report the final commit message for each commit that was created.

## References

- [Git commit authoring guidance](../instructions/git-commit-authoring.instructions.md)
