---
name: "Copilot Files"
description: "Use when creating or editing Copilot files such as instructions, prompts, custom agents, or skills."
applyTo: ".github/copilot-instructions.md,.github/**/*.instructions.md,.github/**/*.prompt.md,.github/**/*.agent.md,.github/**/SKILL.md,AGENTS.md"
---

## Mandatory Read Before Commencing

- Before creating, editing, reviewing, merging, moving, or deleting any Copilot customization file, read the relevant
  official documentation for the file types in scope. And become a guru on the supported formats, frontmatter, and best
  practices.
- Treat these URLs as required pre-work, not optional references.
- Prefer official documentation when examples and docs conflict.

- Custom instructions: `https://code.visualstudio.com/docs/copilot/customization/custom-instructions`
- Prompt files: `https://code.visualstudio.com/docs/copilot/customization/prompt-files`
- Custom agents: `https://code.visualstudio.com/docs/copilot/customization/custom-agents`
- Agent skills: `https://code.visualstudio.com/docs/copilot/customization/agent-skills`
- Awesome Copilot examples and patterns: `https://awesome-copilot.github.com/`

# Copilot guidance

- Choose the file type by purpose.
    - Use `.github/copilot-instructions.md` for always-on shared guidance.
    - Use `.instructions.md` for scoped standards that apply by file type, folder, or semantic match.
    - Use `.prompt.md` for one focused slash-command task.
    - Use `.agent.md` for a persistent persona, tool policy, or handoff workflow.
    - Use `SKILL.md` when the capability is portable, task-specific, and may need scripts, templates, examples, or other
      bundled resources.
- Keep one instruction in one place. When prompts, agents, or skills need shared guidance, link to the relevant
  instruction file instead of copying the same rules.
- Keep frontmatter minimal and within the supported schema for the file type. Remove unsupported metadata instead of
  inventing custom keys.
- Make `description` specific enough to drive discovery. Start with `Use when...` and include the words a user is likely
  to type.
- Keep bodies concise, operational, and reusable. Prefer generic wording and references over repository-specific prose
  unless a rule truly depends on this repository.
- For `.instructions.md`, use `applyTo` only when automatic application is warranted. Avoid broad patterns such as `**`
  unless the guidance is genuinely universal.
- For skills, ensure the `name` matches the parent directory name exactly. Use `user-invocable` and
  `disable-model-invocation` only when you need to control how the skill is discovered.
- For prompts and agents, prefer Markdown links to shared instructions over repeating policy text or coding standards.
