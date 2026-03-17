---
name: "update-readme"
description:
    "Fully rewrite README.md so it accurately documents the project, its current functionality, and its development
    workflow using a concise, well-structured open-source README format."
argument-hint: "Optional hard constraints, sections to preserve, tone preferences, or documentation priorities"
agent: "agent"
---

# README Rewrite

Completely recreate `README.md` based on the library's current functionality.

If the user provides an argument, treat it as a hard constraint or additional priority while still performing the full
rewrite.

## Mission

- Read the current `README.md` before editing it.
- Review the relevant workspace broadly before rewriting so the README reflects the whole project, not just a narrow API
  slice.
- Read the public API surface under `./src` that the README must document.
- Read relevant sample, dev, docs, or test code that reveals intended usage patterns, project structure, and developer
  workflow.
- Rewrite the README around what the library actually does today.
- Replace weak, vague, or placeholder documentation with concise, accurate product documentation.
- Keep the final README comprehensive but still concise and to the point.
- Include inline comments inside example code blocks only when they clarify a non-obvious step, intention, or API usage.

## Workflow

1. Read the current `README.md` and identify gaps, outdated claims, weak sections, and missing industry-standard
   content.
2. Review the repository root and relevant workspace files to understand the project's scope, packaging, tooling, and
   supporting assets.
3. Inspect the public library surface in `./src` and verify the real API names, behavior, and constraints.
4. Read sample, dev, docs, or test code when needed to capture realistic usage examples and developer workflows.
5. Check whether the repository already contains a logo or icon that can be reused in the README header.
6. Rewrite the README so it presents the project clearly for a new consumer.
7. Ensure all code examples use current APIs and compile in principle against the documented surface.
8. Add inline comments to example code only where they improve comprehension.
9. Validate the final README for accuracy, internal consistency, broken claims, formatting issues, and asset references.

## README Standards

- Start with a short description that fits the library's actual purpose.
- Use GitHub Flavored Markdown for formatting.
- Use GitHub admonition syntax when it meaningfully improves clarity, such as for important constraints, tips, or
  caveats.
- Prefer a clean open-source README structure: strong header, short summary, then the most relevant sections in a
  practical order.
- Include professional sections when relevant, such as overview, key features, requirements, installation, quick start,
  core concepts or architecture, usage examples, development workflow, limitations, troubleshooting, or resources.
- Prefer functionality-first documentation over marketing language.
- Keep examples realistic, minimal, and easy to copy.
- Keep the tone professional and direct.
- Keep the document concise; include useful depth, but avoid bloated sections and repetitive explanation.
- Do not include sections that already have dedicated repository files, such as `LICENSE`, `CONTRIBUTING`, or
  `CHANGELOG`.
- If a project logo or icon already exists in the repository, use it in the README header. Do not invent or reference
  assets that are not present.
- When example code contains a non-obvious cast, convention, or behavior, add a short inline comment explaining why.
- Do not document features that are not present in the current codebase.
- Do not leave TODO-style placeholder text.

## Style References

- Take inspiration from these README examples for structure, tone, and concise presentation, while keeping the content
  specific to the current project:
    - `https://raw.githubusercontent.com/Azure-Samples/serverless-chat-langchainjs/refs/heads/main/README.md`
    - `https://raw.githubusercontent.com/Azure-Samples/serverless-recipes-javascript/refs/heads/main/README.md`
    - `https://raw.githubusercontent.com/sinedied/run-on-output/refs/heads/main/README.md`
    - `https://raw.githubusercontent.com/sinedied/smoke/refs/heads/main/README.md`
- Mirror the strengths of those examples rather than copying them: clear header, short value statement, practical
  section ordering, good scanability, and concise examples.
- Do not overuse emojis. If any are used, keep them sparse and intentional.

## Example Commenting Rules

- Add inline comments sparingly inside code blocks.
- Explain intent, conventions, or subtle behavior.
- Do not add comments for obvious assignments, declarations, or straightforward dispatch calls.
- Prefer one useful comment in a snippet over many noisy comments.

## Validation

- Re-read the final `README.md` after editing.
- Check for editor-reported issues in the file.
- If any code examples were changed substantially, verify them against the current source layout and public API names.
- If a logo or icon is used, verify that the referenced asset exists and that the path is correct.
- If something could not be validated, say exactly what was not validated and why.

## Output Expectations

1. Summarize the weaknesses in the original README.
2. Present the rewrite approach before editing when the task is large enough to justify it.
3. After implementation, summarize the major documentation improvements.
4. Report what was validated.
5. Call out any remaining documentation gaps or future README enhancements.
6. Add an index of the final README sections for easy navigation.
