```yaml
name: git-commit
description: >-
    Autonomously generate and run git commits.

allowed-tools: [bash]
metadata:
    autonomous: true
    interactive: false
    intelligent_staging: true
    auto_commit: true
    always_stage_all: false
guidance:
    format: |
        <type>[optional scope]: <description>

        [optional body]

        [optional footer(s)]
    workflow:
        - getStatus: |
              Check the current git status to identify all unstaged changes and determine if there are staged changes that need to be committed. `git status --porcelain`.
        - analyzeDiff: |
              Prefer staged diff (`git diff --staged`) when available; otherwise use
              the working-tree diff (`git diff`). Analyze the diff to understand the nature of changes (e.g., new feature, bug fix, documentation update) and identify affected files and their paths. Use this analysis to inform subsequent staging and commit message generation.
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
        - repeat: |
              If the user has additional unstaged changes after the first commit(s),
              repeat the process to create additional commits until all changes are
              staged and committed.
    bestPractices:
        - "One logical change per commit"
        - "Use imperative present-tense and keep description under 72 chars"
        - "Reference issues with `Closes #NNN` when applicable"
    safety:
        - "Do not modify git config files"
        - "Do not run destructive commands (force push, reset --hard) without explicit user approval"
        - "Do not skip hooks (`--no-verify`) unless user requests it"
```
