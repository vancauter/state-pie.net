# Async Middleware Guidance

Summary

- Middleware may perform asynchronous work, but dispatch semantics are serialized by the store.
- Avoid awaiting long-running work synchronously inside the dispatch pipeline; schedule background work instead.

Recommended helpers

- `SC.AtomicState.Helpers.MiddlewareHelpers.ScheduleBackground(Func<Task>)` — schedules cooperative background work.
- `SC.AtomicState.Helpers.MiddlewareHelpers.SafeDispatchAsync(IDispatchContext, IAction)` — dispatches from async
  contexts safely.

Safe example

```csharp
var mw = Middleware.Create((ctx, next) => action => {
  MiddlewareHelpers.ScheduleBackground(async () => {
    await DoAsyncWork();
    await MiddlewareHelpers.SafeDispatchAsync(ctx, Factory.CreateAction("AFTER"));
  });
  return next(action);
});
```

Unsafe example (avoid)

```csharp
// DO NOT await long-running work on the dispatch stack
var bad = Middleware.Create((ctx, next) => action => {
  await DoAsyncWork(); // blocks dispatch pipeline
  next(action);
});
```
