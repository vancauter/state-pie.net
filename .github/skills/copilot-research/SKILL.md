---
name: "copilot-research"
description:
    "Use when dealing with GitHub Copilot instruction, prompt, skill, and agent customization capabilities, reviewing
    official examples, and creating internal memory from authoritative customization sources."
---

# Copilot Research

Use this skill when the task is to study GitHub Copilot capabilities before changing or creating any instruction,
prompt, agent, or related workflow file.

All workspace customization directories and files created or updated through this skill must live under `./.github/`.

## Use Cases

- Explore and understand custom instruction capabilities.
- Explore and understand prompt file capabilities.
- Review official and community examples before editing customization files.
- Create or refresh internal memory based on authoritative customization documentation.

## Required Sources

Review these sources before making recommendations or edits:

1. Official custom instructions documentation:
   `https://code.visualstudio.com/docs/copilot/customization/custom-instructions`
2. Awesome Copilot examples and patterns: `https://awesome-copilot.github.com/`

If the task also touches prompt files or custom agents, expand the research to the related official VS Code Copilot
documentation for prompts and agents.

## Workflow

1. Gather the current workspace context.
    - Inspect any existing `.instructions.md`, `.prompt.md`, `.agent.md`, `copilot-instructions.md`, `AGENTS.md`, or
      `SKILL.md` files under `./.github/` that may be affected.
    - Identify whether the task is about instructions, prompts, agents, or skills.
2. Read the authoritative documentation.
    - Start with the official VS Code documentation.
    - Use Awesome Copilot as a secondary source for examples and patterns.
    - Prefer official documentation when examples and docs conflict.
3. Extract the relevant capability model.
    - Supported file locations.
    - Supported frontmatter fields.
    - Discovery behavior and triggering rules.
    - Any file-type-specific constraints or best practices.
4. Create or update internal memory.
    - First inspect the existing memory directory to avoid duplicates.
    - Store only durable facts that will help with future Copilot customization tasks.
    - Keep the memory concise and practical.
5. Apply the findings.
    - Normalize or create files using the officially supported structure under `./.github/`.
    - Preserve the actual intent of existing instructions unless the user asks for behavioral changes.
6. Validate the result.
    - Check for frontmatter or diagnostics errors.
    - Confirm references such as agent names match the actual registered names in the workspace.

## Memory Guidance

When creating memory from the researched sources, record short facts such as:

- Where each customization type lives.
- Which frontmatter keys are officially supported.
- How the agent discovers instructions, prompts, agents, or skills.
- Which source is authoritative when examples vary.

Do not store long summaries or duplicate the documentation verbatim.

## Output Expectations

1. Summarize the relevant Copilot customization capabilities.
2. Call out any constraints that affect the requested file type.
3. State that internal memory was created or updated, if memory work was part of the task.
4. Then implement the requested customization changes.

## Quality Bar

- Keep all workspace customization directories and files under `./.github/`.
- Use official formats and headers.
- Keep frontmatter valid YAML.
- Keep descriptions specific enough to improve discovery.
- Preserve user intent while improving structure and consistency.
