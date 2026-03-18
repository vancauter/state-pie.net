# Workspace Guidance

## Project Facts

- AtomicState is a pre-release project. Breaking changes are acceptable when they materially improve the API, naming, or
  structure.
- `src/` contains the library, `tests/` contains xUnit coverage, `dev/` contains the runnable dev harness, and `docs/`
  contains supporting notes.
- AtomicState is a reactive state container for .NET built around actions, reducers, stores, slices, and middleware.
- The target platform is .NET 10 with nullable reference types and implicit usings enabled.
- Reducers are expected to be pure and should return the existing state instance when nothing changed.

## Validation

- Prefer the existing workspace tasks for routine validation.
- Default build task: `Build Solution`
- Default test task: `Run Tests`
- Performance validation is opt-in via the `Run Performance Tests` task.

## Working Rules

- Understand the existing code, tests, and public surface before changing behavior or structure.
- Prefer the smallest change that fixes the root problem and fits the current codebase.
- Keep changes proportionate to the project. Do not add layers, abstractions, or files unless they clearly improve the
  result.
- Preserve public contracts unless the task explicitly calls for a breaking change.
- When behavior changes, update the relevant tests and validate with the narrowest relevant build or test command or
  task.
- When documentation or examples describe changed behavior, update them in the same pass.
- Use the scoped instruction files for language-, file-, and customization-specific rules instead of duplicating those
  rules here.
