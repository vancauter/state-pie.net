# StatePie Roadmap

Internal planning notes for capabilities not yet implemented. This is a set of candidates, not a commitment to implement
every item.

## Remaining Candidates

### Async Effects

- Define how asynchronous work is started outside reducers and how its result returns as a typed action.
- Specify cancellation, error reporting, store disposal, and dispatch ordering before adding async APIs.
- Keep the current middleware contract synchronous; do not turn middleware into an underspecified background-work
  mechanism.

## Later Candidates

### StateMerger Performance Options

- Benchmark reflection-based merging against direct reducer updates in representative workloads.
- Add a non-reflection option only if measurements show a meaningful need.
- Document when direct immutable state construction is preferable to StateMerger.

### Developer Tooling

- Explore optional development-time transition inspection and structured output for external tools.
- Keep tooling separate from the core store contract unless a stable, broadly useful extension point emerges.

### Strict Validation Mode

- Consider opt-in runtime checks for invalid reducer outputs or inconsistent slice state.
- Keep default behavior small and predictable; add a mode only where it catches errors the type system and current
  validation cannot.

## Recommended Next Step

Complete a release-readiness pass before adding another core feature: validate the public API through the runnable
examples, review package metadata and documentation, and run the Release build and full test suite. The current
synchronous store, typed middleware, selectors, and disposable subscriptions form a coherent core; async effects add
cancellation, failure, and lifetime policy that should wait for a concrete user need.

## Out Of Scope For Core

- Replacing UI framework state systems end-to-end.
- Owning host-specific asynchronous scheduling or all side-effect execution semantics.
- Becoming an event-sourcing framework.
