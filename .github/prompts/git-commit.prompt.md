```yaml
name: git-commit
description: >-
    Generate and run git commits using the Conventional Commits format. The
    assistant should use this prompt when the user asks to create a commit,
    mentions `/commit`, or requests an intelligent commit message based on the
    repo diff. Supports auto-detection of type/scope, interactive overrides,
    and sensible staging guidance.
allowed-tools: [bash]
metadata:
    conventional_commits: true
    interactive: false
    intelligent_staging: true
    auto_commit: true
    always_stage_all: false
    autonomous: true
guidance:
    format: |
        <type>[optional scope]: <description>

        [optional body]

        [optional footer(s)]
    workflow:
        - analyzeDiff: |
              Prefer staged diff (`git diff --staged`) when available; otherwise use
              the working-tree diff (`git diff`). Also check `git status --porcelain`.
        - stageFiles: |
              Analyze the working-tree diff and group changed files into logical
              cohorts (eg. `docs`, `api`, `feature:<scope>`, `tests`, `ci`). For
              each logical group, create a separate staging set; avoid staging
              files that appear to contain secrets (private keys, env files,
              credentials). Do not implicitly stage unrelated groups together.
              The assistant should autonomously decide groups and prepare per-
              group commits.
        - generateMessage: |
              Derive `type` (feat, fix, chore, docs, refactor, test, ci), `scope` from
              touched paths, and a short imperative description (<72 chars). Offer an
              optional body and footers (e.g., `Closes #NNN`).
              When multiple groups are selected, generate a separate conventional
              commit message for each group. Do NOT automatically combine unrelated
              groups into a single commit.
        - executeCommit: |
              For each autonomous logical group produced by `stageFiles`, stage
              only that group's files and create a separate conventional commit
              with an appropriate type/scope and message. Prefer multi-line
              commits when a body or footers are included. Do not require user
              confirmation before running `git commit`. Preserve safety rules
              (do not modify git config, do not run destructive commands, do
              not skip hooks without explicit user request).
    bestPractices:
        - "One logical change per commit"
        - "Use imperative present-tense and keep description under 72 chars"
        - "Reference issues with `Closes #NNN` when applicable"
    safety:
        - "Do not modify git config files"
        - "Do not run destructive commands (force push, reset --hard) without explicit user approval"
        - "Do not skip hooks (`--no-verify`) unless user requests it"
```
