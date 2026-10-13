# StatePie

StatePie is an opinionated .NET state management library for predictable, immutable application state updates using
strongly typed actions, reducers, and slices. It enforces unidirectional state updates through typed actions,
reducer-driven transitions, and immutable state snapshots.

## Features

- Composes app state from named slices keyed in a single state snapshot dictionary.
- Enforces one action family per store through generic typing.
- Runs all slice reducers for each dispatch and builds a new root snapshot only when needed.
- Uses reference equality to treat unchanged reducer outputs as no-op updates.
- Publishes root or selected state changes through disposable subscriptions.
- Includes StateMerger for pragmatic immutable updates via cached reflection-based property overlay.

## Design Philosophy

- Predictability over magic: all state changes are explicit in reducers.
- Immutability over in-place mutation: easier reasoning, safer event behavior, and cheap change checks.
- Compile-time constraints over string routing: typed actions reduce accidental misuse and make refactors safer.
- Centralized composition rules: store creation validates slice names, null guards, and consistent action typing once.

## Practical Use Cases

- Desktop app state (WPF, WinUI, MAUI) where multiple screens/components must react to shared state consistently.
- Long-lived workflow state, such as wizard flows, editors, form builders, or order configs.
- Domain-heavy applications that benefit from explicit, testable reducer logic for business transitions.
- Teams that prefer strict architecture boundaries and deterministic event ordering.
- Projects that want Redux-like mental models in .NET without adding a large framework.

## When Not To Use StatePie

- Simple CRUD apps with mostly local, short-lived state where plain objects or minimal MVVM state is enough.
- Ultra-high-frequency update loops (for example game loops or telemetry at very high rates) where reducers fan-out and
  snapshot creation overhead may be unnecessary.
- Event-sourced systems that already treat the event log as source of truth and do not need an in-memory reducer store
  abstraction.
- Teams that prefer direct mutable models and do not want reducer discipline.
- Scenarios requiring fine-grained reactive graph propagation per field instead of snapshot-based store updates.

## Tradeoffs

- You gain predictable transitions and safer refactors, but you write more boilerplate than ad-hoc mutable state.
- You gain clear test seams around reducers, but you must design action types intentionally.
- You gain deterministic store composition, but the architecture is less flexible for free-form mutation patterns.
