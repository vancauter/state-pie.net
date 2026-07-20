---
name: copilot-authoring.instructions
description: Use when creating or editing Copilot customization files (instructions, prompts, agents, skills)
applyTo: .github/copilot-instructions.md,.github/**/*.instructions.md,.github/**/*.prompt.md,.github/**/*.agent.md,.github/**/SKILL.md,AGENTS.md
---

# Purpose

Define strict, non-overlapping rules for structuring Copilot customization files.

Ensures:

- Clear responsibility boundaries
- Zero duplication across file types
- Consistent, maintainable configuration

# Prerequisites

Before modifying any Copilot customization file:

- Read the official documentation for the relevant file type
- Treat documentation as authoritative over examples
- Validate supported frontmatter before adding fields

# Core Model

Each file type has exactly one responsibility:

- **Instructions** → Always-on rules and context
- **Agent** → Behavior and execution strategy
- **Prompt** → Explicit task definition
- **Skill** → Conditional capability

Rules:

- One responsibility per file type
- No duplication across types
- Prefer linking over copying

# File Type Rules

## Instructions Files

Paths:

- `.github/copilot-instructions.md`
- `.github/instructions/*.instructions.md`

### Use when

- Guidance applies to most interactions (≈80%+)

### Must

- Follow the instructions template
- Define global conventions and rules
- Provide shared decision frameworks
- Remain passive and always applicable

### Must not

- Define workflows
- Include persona or behavior
- Include conditional logic

## Prompt Files

Path:

- `.github/prompts/*.prompt.md`

### Use when

- Defining a single, explicitly invoked task

### Must

- Follow the prompt template
- Define task intent (“what to do”)
- Accept structured input (`${input:...}`)
- Define output format

### Must not

- Include global rules
- Include reusable capabilities
- Execute implicitly

## Agent Files

Path:

- `.github/agents/*.agent.md`

### Use when

- Defining how tasks are executed

### Must

- Follow the agent template
- Define persona or role
- Define tool usage policy
- Define execution strategy

### Must not

- Define task content
- Include project or domain rules
- Include specialized procedures

## Skill Files

Path:

- `.github/skills/*/SKILL.md`

### Use when

- Providing a specialized, conditional capability

### Must

- Follow the skill template
- Be narrowly scoped
- Define “when to use”
- Provide execution steps and optional resources

### Must not

- Contain global rules
- Be required for most tasks
- Duplicate prompt workflows

# Decision Rules

| Question                                | File Type    |
| --------------------------------------- | ------------ |
| Should this always apply?               | Instructions |
| Is this explicitly triggered?           | Prompt       |
| Does this define behavior or execution? | Agent        |
| Is this conditional or specialized?     | Skill        |

# Authoring Guidelines

## General

- Keep content concise and reusable
- Prefer references over duplication
- Avoid unnecessary repository-specific wording

## Frontmatter

- Use only supported fields
- Keep minimal and valid

## Descriptions

- Start with: **“Use when…”**
- Include searchable keywords

## Instructions Scope

- Use `applyTo` only when justified
- Avoid overly broad patterns unless truly global

## Skills

- Directory name must match skill name
- Use invocation controls only when necessary

## Prompts and Agents

- Link to instructions instead of repeating rules

# Anti-Patterns

- Duplicating rules across file types
- Defining workflows in instructions
- Putting standards in prompts
- Embedding persona in prompts
- Using skills as general-purpose logic

# Mental Model

- **Instructions** → Operating system
- **Agent** → Execution mode
- **Prompt** → Command
- **Skill** → Plugin

# References

## Required References

- Custom instructions: https://code.visualstudio.com/docs/copilot/customization/custom-instructions
- Prompt files: https://code.visualstudio.com/docs/copilot/customization/prompt-files
- Custom agents: https://code.visualstudio.com/docs/copilot/customization/custom-agents
- Agent skills: https://code.visualstudio.com/docs/copilot/customization/agent-skills
- Examples: https://awesome-copilot.github.com/
