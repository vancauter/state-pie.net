# Workspace Guidance

## Project Facts

- Status: Pre-release, early development. Breaking changes are expected and tolerated.
- This project is not meant for open-source. Ignore any "public API" or "external contribution" considerations. Focus on
  internal consistency, clarity, and maintainability.

## Structure and Conventions

- `src/` contains the library
- `tests/` contains xUnit coverage
- `dev/` contains the runnable dev harness
- `docs/` contains supporting notes, design docs, and code review records.
- `scripts/` contains build and validation scripts.
- `third_party/` contains vendored dependencies that are not available via NuGet.
- `third_party/ggufs/` contains the GGUF files for the LLMs used in development and testing.

## Rules

- Always use VSCode IDE tools first when possible, then copilot agents', and only use external scripts as a last resort.
- Do not use Python, Shell or other external scripts unless absolutely necessary.
- Always check for relevant instructions before responding. If instructions exist, follow them rigorously.

## Validation

- Check and read: '.github' for relevant skills and instructions, and memory, before starting work on a task or
  response.
- Prefer the existing workspace tasks for routine validation.
- Default build task: `Build Solution`
