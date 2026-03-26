---
name: total-refactor-plan
description: Plan and execute a complete, modern refactor of a .NET library or codebase.
agent: deep-research.agent
model: GPT-5 mini
---

This prompt instructs the agent to design and plan a full refactor of a .NET library or codebase.

Goals

- Produce a modern, modular, well-named, SOLID-aligned project structure appropriate to the repository's size.
- Breaking changes are permitted; NO need to document breaking API changes or provide migration guidance.
- Improve testability, observability, maintainability, and (where sensible) runtime characteristics.
- Ensure the repository's primary build and test commands run successfully after the refactor.

Required scope

- Analyze the current public surface and internal implementation (commonly under `src/` or the repository root).
- Propose a new project/module layout (split into logical projects or folders where helpful).
- Rename, split, and reorganize files and folders to industry-standard names; group related interfaces by subject.
- Reduce coupling: create small, focused interfaces and apply Dependency Inversion where appropriate.
- Prefer pure/static functions for deterministic helpers when it improves clarity and testability.
- Update tests, examples, and developer harnesses to reflect the new structure.
- Update documentation and any `SKILL.md` or repository-guidance files that reference public API names.
- Include rationale for major decisions and a comprehensive list of all files renamed, moved, or deleted.

Deliverables

- `docs/design/<name-project>-design-v<version>.md` — full design plan and change list. Use existing design document
  formats in the repository as a template if available, an normalize filename accordingly.
