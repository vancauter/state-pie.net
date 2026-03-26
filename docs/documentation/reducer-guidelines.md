# Reducer Guidelines

Summary

- Reducers must preserve reference identity when no change occurs. Returning a new object that is value-equal but not
  reference-equal causes unnecessary publications and breaks identity-based optimizations.

Rules

- If the action does not change state, return the same instance.
- For record/immutable types use `with` (or create a new instance) only when a real change is required.
- Avoid shallow copies that produce logically equal but distinct instances.

Anti-pattern

```csharp
// BAD: returns a new instance even when values are unchanged
if (!needsChange) return new MyState(old.Value, old.Other);
```

Good pattern

```csharp
// GOOD: return existing instance when unchanged
if (!needsChange) return old;

return old with { Value = newValue };
```
