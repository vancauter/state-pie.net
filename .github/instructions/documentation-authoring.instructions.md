---
name: "documentation-authoring"
description:
    "Use when writing or revising README files and repository Markdown documentation so the content matches the current
    codebase, follows Diataxis-style boundaries, and stays practical for the target audience."
applyTo: "README.md,docs/**/*.md,examples/**/*.md,CONTRIBUTING.md,CHANGELOG.md,CODE_OF_CONDUCT.md,SECURITY.md,SUPPORT.md"
---

# Documentation authoring guidance

- Choose the document type that best fits the request.
    - Tutorial: teach by guiding the reader through a concrete outcome.
    - How-to: help the reader complete a task quickly and reliably.
    - Reference: document facts, interfaces, commands, and behaviors precisely.
    - Explanation: clarify concepts, trade-offs, design choices, and mental models.
- Keep those document types distinct when practical. Small repository documents may blend them, but do not bury a task
  flow inside a purely conceptual section or hide reference details in a tutorial narrative.
- Identify the target audience, reader goal, and scope before drafting. If those materially affect the result and are
  unclear, resolve the ambiguity first.
- Treat the repository as the source of truth. Read the relevant code, tests, runnable entry points, tasks, and docs
  before writing or revising documentation.
- For repository documentation work, audit the surrounding documentation set instead of treating the target file in
  isolation. Decide which content belongs in the landing page, deeper docs, examples, or community-health files.
- Use `README.md` as the landing page for purpose, status, quick start, high-value usage guidance, and links to deeper
  material. Do not turn the README into the only place where every detail lives.
- Place deeper material where readers expect to find it.
    - `docs/`: how-to guides, explanations, architecture notes, operational guidance, migration notes, and detailed
      reference material.
    - `examples/`: runnable or copy-adaptable usage examples and tutorials when examples materially improve onboarding.
    - Root community files such as `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`, `SUPPORT.md`, and
      `CHANGELOG.md`: contributor and repository-health guidance that should be discoverable from GitHub.
- Create or recommend companion documents only when the repository actually needs them. Typical candidates include API
  reference docs, installation/setup guides, examples, contributing guidance, changelogs, migration notes, and security
  reporting guidance.
- Treat API reference documentation as conditional, not automatic. Add a dedicated API doc when the public surface is
  broad enough that the README would become overloaded or the code-level docs are not sufficient for consumers.
- Prefer concise, scannable Markdown with purposeful headings, short paragraphs, and examples that are realistic enough
  to validate mentally.
- State prerequisites, assumptions, and environment constraints near the start of task-oriented documents.
- Keep examples minimal but complete enough to copy or adapt without guessing.
- Verify commands, file paths, links, and referenced assets as far as practical.
- Prefer relative links between repository documents so links work on GitHub and in local clones.
- Do not document features, options, guarantees, or workflows that are not present in the current codebase.
- When rewriting an existing document, preserve accurate sections that still add value and replace stale sections
  decisively instead of layering contradictory text on top.
- When documenting a workflow, include the expected verification signal when one exists.
- Update only the sections affected by the change unless the document is clearly stale enough to justify a broader
  rewrite.
