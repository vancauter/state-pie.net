---
name: git-commit
description: Use when creating one or more git commits for the current workspace.
agent: git-commit.agent
model: GPT-5 mini
---

# Purpose

Create one or more git commits for the current workspace. Work autonomously to group staged changes into logical
commits, and generate a commit message for each group using the Conventional Commits format. And then commit each group
to the repository.

# Output

- A concise summary of logical commit groups.
- The final commit message(s) for each staged group, formatted with Conventional Commits.
- All groups committed to the repository.
