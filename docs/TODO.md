# AtomicState Current Action Plan

## Objective

Keep AtomicState aligned with its documented scope as a lightweight reactive state container, while removing actions
that are already covered by the current implementation and test suite.

## Priority Actions

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
