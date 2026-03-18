---
name: "documentation-authoring"
description:
    "Use when writing or revising README files and Markdown documentation so the content matches the current codebase,
    follows Diataxis-style boundaries, and stays practical for the target audience."
applyTo: "README.md,docs/**/*.md"
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
- Prefer concise, scannable Markdown with purposeful headings, short paragraphs, and examples that are realistic enough
  to validate mentally.
- State prerequisites, assumptions, and environment constraints near the start of task-oriented documents.
- Keep examples minimal but complete enough to copy or adapt without guessing.
- Verify commands, file paths, links, and referenced assets as far as practical.
- Do not document features, options, guarantees, or workflows that are not present in the current codebase.
- When rewriting an existing document, preserve accurate sections that still add value and replace stale sections
  decisively instead of layering contradictory text on top.
- When documenting a workflow, include the expected verification signal when one exists.
- Update only the sections affected by the change unless the document is clearly stale enough to justify a broader
  rewrite.
