---
name: "sc-repository-docs"
description:
    "Use when auditing and updating README.md plus related repository documentation such as docs/, examples/, API docs,
    and community-facing Markdown files."
argument-hint: "Hard constraints, docs to preserve, missing-doc concerns, tone preferences, or documentation priorities"
agent: "Documentation Expert"
---

# Repository Docs Update

Audit and update the repository's user-facing documentation so it accurately matches the current codebase.

## Workflow

1. Read the current `README.md` and inventory the existing documentation set before editing.
2. Inspect the relevant source, tests, samples, docs, and runnable entry points so the documentation reflects the real
   project.
3. Decide which documentation is actually needed for this repository and where it belongs.
    - `README.md`: landing page, status, quick start, high-value usage guidance, and links outward.
    - `docs/`: deeper how-to, explanation, architecture, installation, migration, troubleshooting, or reference docs.
    - `examples/`: runnable or copy-adaptable usage examples when they materially improve onboarding.
    - Repository-level companion docs such as `CONTRIBUTING.md`, `CHANGELOG.md`, `SECURITY.md`, `SUPPORT.md`, or an API
      reference doc: create or update them only when they are justified by the repository's real needs.
        - GitHub-standard repository companion docs belong in their own root files when they exist. Do not fold their
          content into `README.md`, and do not treat README sections as substitutes for those files.
4. Make an explicit call on missing-doc candidates instead of guessing.
    - Say whether dedicated API reference documentation is warranted.
    - Say whether contributing, changelog, security, support, or examples docs are warranted.
    - Say whether a deeper `docs/` split is warranted or whether the README should stay the primary document.
5. Prefer updating existing docs over creating parallel documents, and keep overlap low when multiple docs are needed.
6. Validate links, commands, paths, and referenced assets as far as practical.
7. If you encounter a code error or compile error, stop and report it instead of debugging it or making implementation
   changes.

## Output expectations

1. Start with a documentation audit summary that names the missing, stale, redundant, or misplaced docs in scope.
2. State the proposed documentation set and where each document should live.
3. Call out when GitHub-standard companion docs should remain separate from `README.md`.
4. Explain which candidate documents are justified, which are unnecessary for now, and why.
5. Summarize the major documentation updates after editing.
6. Report what was validated and what could not be validated.

## References

- [Documentation authoring guidance](../instructions/documentation-authoring.instructions.md)
