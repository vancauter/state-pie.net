# AtomicState Current Action Plan

## Objective

Keep AtomicState aligned with its documented scope as a lightweight reactive state container, while removing actions
that are already covered by the current implementation and test suite.

## Priority Actions

### P1: Make initialization failure behavior explicit

- [ ] Replace the blanket exception suppression in the store `@@INIT` path with an intentional contract.
- [ ] Decide whether initialization failures should fail store creation, be surfaced through diagnostics, or use an
      explicit opt-in error callback.
- [ ] Update tests to verify the chosen initialization failure contract for reducer and middleware exceptions.

Why this matters: `Store.RunInitPass()` currently swallows every exception, which can leave callers with a usable store
object but no signal that initialization logic failed.

### P2: Add a supported combined-slice access API

- [ ] Add a public API for retrieving combined slice state by slice name and expected type.
- [ ] Update docs and examples to use that API instead of recommending serialization or test-only reflection helpers.
- [ ] Remove reflection-based access patterns from tests where a public accessor can validate the intended contract.

Why this matters: combined state remains an internal runtime shape today, and even the test suite relies on reflection
to read slices. That is a strong signal that the public API is still missing an ergonomic read path.

### P3: Align installation docs with shipping reality

- [ ] Remove or replace the placeholder `dotnet add package AtomicState` guidance until package metadata and delivery
      are actually defined.
- [ ] Add the real distribution path for consumers, whether that is NuGet packaging metadata or source/project reference
      guidance.
- [ ] Review README wording so release-readiness claims do not overstate what is currently available.

Why this matters: the README still presents package installation as planned placeholder text, while the project file has
no packaging metadata. That creates avoidable confusion for first-time consumers.

## Exit Criteria

- Initialization failures have a documented and tested contract.
- Consumers can read combined slice state through a supported public API.
- Installation guidance matches the way AtomicState is actually shipped.
