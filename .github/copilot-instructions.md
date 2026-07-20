# Workspace Guidance

## Structure and Conventions

- `dev/` contains the runnable dev harness
- `docs/` contains supporting notes, design docs, and code review records.
- `examples/` contains example code and usage patterns.
- `scripts/` contains build and validation scripts.
- `src/` contains the source code for the library/app, organized by feature area.
- `tests/` contains xUnit coverage
- `third_party/` contains vendored dependencies that are not available via NuGet.

## Tips

- Replacing a file with new file, is often way faster than patching one. So if it's a better option, do that instead.

## Rules

- Always use VSCode IDE tools first when possible.
- Do not use Python, shell, or other external scripts unless necessary or for small, quick updates.
- Always check for relevant instructions before responding. If instructions exist, follow them rigorously.

## Validation

- Check and read: '.github' for relevant skills and instructions, and memory, before starting work on a task or
  response.
- Prefer the existing workspace tasks for routine validation.
- Default build task: `Build Solution`
