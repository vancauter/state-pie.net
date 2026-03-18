---
name: "copilot-research"
description:
    "Use when researching GitHub Copilot instructions, prompts, custom agents, or skills before creating or changing
    customization files."
argument-hint: "Describe which customization types are in scope and what you want to change"
---

# Copilot Research

Use this skill when the task is to study GitHub Copilot customization capabilities before changing or creating any
instruction, prompt, agent, skill, or related workflow file.

All workspace customization files created or updated through this skill must live under `./.github/`.

## Required sources

Review the relevant official VS Code documentation before making recommendations or edits:

1. Custom instructions: `https://code.visualstudio.com/docs/copilot/customization/custom-instructions`
2. Prompt files: `https://code.visualstudio.com/docs/copilot/customization/prompt-files`
3. Custom agents: `https://code.visualstudio.com/docs/copilot/customization/custom-agents`
4. Agent skills: `https://code.visualstudio.com/docs/copilot/customization/agent-skills`
5. Awesome Copilot examples and patterns: `https://awesome-copilot.github.com/`

Prefer official documentation when examples and docs conflict.

## Workflow

1. Inspect the current workspace context.
    - Read any affected `.instructions.md`, `.prompt.md`, `.agent.md`, `copilot-instructions.md`, `AGENTS.md`, or
      `SKILL.md` files under `./.github/`.
    - Identify which concerns are already present and where duplication exists.
2. Build the capability model from the docs.
    - Supported locations.
    - Supported frontmatter.
    - Discovery behavior and triggering rules.
    - File-type-specific constraints and best practices.
3. Decide which primitive each concern belongs to.
    - Always-on shared guidance: `copilot-instructions.md` or `AGENTS.md`.
    - Scoped standards or conventions: `.instructions.md`.
    - Focused reusable slash command: `.prompt.md`.
    - Persistent persona, tool policy, or handoffs: `.agent.md`.
    - Portable task workflow with optional bundled resources: `SKILL.md`.
4. Update internal memory.
    - Inspect memory first to avoid duplicates.
    - Store only durable facts that help with future Copilot customization work.
5. Apply the findings.
    - Normalize or create files using the supported structure under `./.github/`.
    - Preserve the intent of existing files unless the user asks for behavioral changes.
6. Validate the result.
    - Check frontmatter and diagnostics.
    - Confirm references such as agent names and linked files still resolve correctly.

## Output expectations

1. Summarize the relevant Copilot customization capabilities.
2. Call out the constraints that affect the requested file types.
3. State whether internal memory was created or updated.
4. Then implement the requested customization changes.

## Quality bar

- Keep all workspace customization files under `./.github/`.
- Use supported formats and frontmatter only.
- Keep descriptions specific enough to improve discovery.
- Avoid duplicating shared guidance across prompts, agents, skills, and instructions.
