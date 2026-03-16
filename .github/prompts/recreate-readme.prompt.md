---
name: "recreate-readme"
description: "Fully rewrite README.md so it accurately documents the library, its usage, and its development workflow."
argument-hint: "Optional constraints, sections to preserve, tone preferences, or documentation priorities"
agent: "csharp-expert"
---

Completely recreate `README.md` based on the library's current functionality.

If the user provides an argument, treat it as a hard constraint or additional priority while still performing the full
README rewrite.

Core mandate:

- Read the current `README.md` before editing it.
- Read the public API surface under `./src` that the README must document.
- Read any relevant sample, dev, or test code that reveals intended usage patterns.
- Rewrite the README around what the library actually does today, not around aspirational or generic framework copy.
- Replace weak, vague, or placeholder documentation with concise, accurate product documentation.
- Include inline comments inside example code blocks when they clarify a non-obvious step, intention, or API usage.

Required workflow:

1. Read the current `README.md` and identify gaps, outdated claims, weak sections, and missing industry-standard
   content.
2. Inspect the current public library surface in `./src` and verify the real API names, behavior, and constraints.
3. Read sample or test code when needed to capture realistic usage examples and developer workflows.
4. Rewrite the README so it presents the library clearly for a new consumer.
5. Ensure all code examples use current APIs and compile in principle against the documented surface.
6. Add inline comments to example code only where they improve comprehension. Do not comment every line.
7. Validate the final README for accuracy, internal consistency, broken claims, and formatting issues.

README standards:

- Start with a short description that fits the library's actual purpose.
- Include the sections that are normally expected in a professional library README when they are relevant, such as:
    - overview
    - key features
    - requirements
    - installation
    - quick start
    - core concepts or architecture
    - usage examples
    - development or local workflow
    - roadmap or limitations
    - contributing
    - license
- Prefer functionality-first documentation over marketing language.
- Keep examples realistic, minimal, and easy to copy.
- When example code contains a non-obvious cast, convention, or behavior, add a short inline comment explaining why.
- Do not document features that are not present in the current codebase.
- Do not leave TODO-style placeholder text in the README.
- Keep the tone professional and direct.

Example-code commenting rule:

- Add inline comments sparingly inside code blocks.
- Explain intent, conventions, or subtle behavior.
- Do not add comments for obvious assignments, declarations, or straightforward dispatch calls.
- Prefer one useful comment in a snippet over many noisy comments.

Minimum validation:

- Re-read the final `README.md` after editing.
- Check for editor-reported issues in the file.
- If any code examples were changed substantially, verify them against the current source layout and public API names.
- If something could not be validated, say exactly what was not validated and why.

Output requirements:

1. Summarize the weaknesses in the original README.
2. Present the README rewrite approach before editing when the task is large enough to justify it.
3. After implementation, summarize the major documentation improvements.
4. Report what was validated.
5. Call out any remaining documentation gaps or future README enhancements.
