---
name: "sc-write-readme"
description: "Use when auditing and updating README.md."
argument-hint: "Hard constraints, docs to preserve, missing-doc concerns, tone preferences, or documentation priorities"
agent: "Documentation Expert"
---

# Repository Docs Update

Write or update the repository's `README.md` so it accurately matches the current codebase. Do not create, update, or
recommend any other files.

## Workflow

1. Read the current `README.md` before editing.
2. Inspect the relevant source, tests, samples, and runnable entry points so the documentation reflects the real
   project.
3. Review ./docs/internal/ for any relevant design review notes that should be reflected in the public docs.
4. Write or update only the `README.md` file. Do not create, update, or recommend any other documentation files,
   companion docs, or examples.
5. Validate links, commands, paths, and referenced assets as far as practical.
6. DO not add standard GitHub companion files or reference them.
7. If you encounter a code error or compile error, stop and report it instead of debugging it or making implementation
   changes.

## Output expectations

1. Start with a documentation audit summary that names the missing, stale, redundant, or misplaced docs in scope.
2. State the proposed documentation set and where each document should live.
3. Call out when GitHub-standard companion docs should remain separate from `README.md`.
4. Explain which candidate documents are justified, which are unnecessary for now, and why.
5. When official companion files were fetched or adapted, name their source and where they were placed.
6. Summarize the major documentation updates after editing.
7. Report what was validated and what could not be validated.

## References

- [Documentation authoring guidance](../instructions/documentation-authoring.instructions.md)
- [GitHub default community health files](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/creating-a-default-community-health-file)
