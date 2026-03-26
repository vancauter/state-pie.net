---
name: total-refactor-plan
description: Architect and plan a modern .NET codebase transformation using deep research.
agent: deep-research.agent
model: GPT-5 mini
---

### Instructions

You are tasked with designing a comprehensive structural and architectural refactor for a .NET repository. Conduct
online research into modern .NET best practices and the specific library domain before finalizing the plan.

#### 1. Analysis & Strategy

- **Research:** Perform deep research on the current repository size, public API surface, and internal implementation.
- **Modernization:** Define a modular, SOLID-aligned architecture. Prioritize testability, maintainability, and
  observability.
- **Logic:** Favor pure, static functions for deterministic helpers and apply Dependency Inversion to decouple
  components.

#### 2. Structural Reorganization

- **Project Layout:** Propose a new project/folder hierarchy (e.g., splitting monolithic blocks into logical modules).
- **Refactor Mapping:** Rename, split, and move files/folders to meet industry standards. Group interfaces by subject
  matter.
- **Test Alignment:** Ensure all tests, examples, and developer harnesses are updated to match the new structure.
- **Breaking Changes:** You are authorized to make breaking changes. **Do not** spend effort documenting API migration
  guides or breaking change lists.

#### 3. Documentation & Integrity

- **Guidance:** Update all `SKILL.md` files, repository-level documentation, and public API references.
- **Validation:** The plan must ensure that primary build and test commands remain functional post-refactor.
- **Rationale:** Provide clear technical justification for major architectural shifts.

---

### Deliverables

**Version Convention:** Select the next logical semantic version (e.g., `v0.3.0`) and use it consistently.

- **Design Document:** Create `docs/design/*-design-v<version>.md`.
    - **Format:** Mirror the structure and naming convention of existing repository design docs.
    - **Content:** Include the full architectural plan and a definitive manifest of all moved, renamed, or deleted
      files.
