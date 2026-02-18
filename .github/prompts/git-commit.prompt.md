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
    interactive: true
    intelligent_staging: true
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
              Propose *grouped* staging rather than staging everything at once. The
              assistant should:

              1. Analyze changed paths and suggest logical groups (by directory,
                  feature, or file type). Example: `src/StateContainer/*` -> `state` scope,
                  `dev/*` -> `dev` scope, `docs/*` -> `docs` scope.
              2. Present the user with the proposed groups and the files in each group
                  and ask which groups to stage/commit now.
              3. For each selected group, emit the exact `git add` commands (or
                  `git add -p` suggestion) to stage only those files.
              4. Never stage secrets (env, keys) and highlight any potential secrets
                  detected in the diff for user review.
        - generateMessage: |
              Derive `type` (feat, fix, chore, docs, refactor, test, ci), `scope` from
              touched paths, and a short imperative description (<72 chars). Offer an
              optional body and footers (e.g., `Closes #NNN`).
                  When multiple groups are selected, generate a separate conventional
                  commit message for each group. Do NOT automatically combine unrelated
                  groups into a single commit.
        - executeCommit: |
              For each group to commit, provide the exact `git commit` command to run.
              Prefer multi-line commits when a body or footers are included. You don't need
              user confirmation before running any `git commit` commands when this prompt is used.
    bestPractices:
        - "One logical change per commit"
        - "Use imperative present-tense and keep description under 72 chars"
        - "Reference issues with `Closes #NNN` when applicable"
    safety:
        - "Do not modify git config files"
        - "Do not run destructive commands (force push, reset --hard) without explicit user approval"
        - "Do not skip hooks (`--no-verify`) unless user requests it"
```
