---
name: "sc-readme-update"
description: "Use when rewriting README.md so it matches the current repository, workflow, and public surface."
argument-hint: "Hard constraints, sections to preserve, tone preferences, or documentation priorities"
agent: "Documentation Expert"
---

# README Update

Rewrite `README.md` so it accurately documents the current repository.

## Workflow

1. Read the current README and identify stale claims, weak sections, and missing essentials.
2. Inspect the relevant source, tests, samples, docs, and runnable entry points so the README reflects the real project.
3. Match the README structure to the repository type and keep explanation, quick-start, and reference content clearly
   separated where practical.
    - Library or package: overview, install, quick start, core concepts, API usage, development.
    - Application or service: overview, prerequisites, setup, run, configuration, architecture, operations.
    - Tooling or template repo: purpose, prerequisites, commands, generated output, development workflow.
4. Keep examples realistic, minimal, and easy to copy.
5. Do not document features that are not present in the current codebase.
6. Validate links, commands, and referenced assets as far as practical.

## Output expectations

1. Summarize the weaknesses in the original README.
2. Present the rewrite approach when the change is substantial.
3. Summarize the major documentation improvements.
4. Report what was validated and what could not be validated.

## References

- [Documentation authoring guidance](../instructions/documentation-authoring.instructions.md)
