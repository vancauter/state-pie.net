# Async Middleware Guidance

- Synchronous nested dispatch is supported and runs inside the store's serialized pipeline.
- Do not `await` long-running work inside the dispatch pipeline; prefer scheduling background work.
- Use `MiddlewareHelpers.ScheduleBackground(Func<Task>)` to run async work off the dispatch stack.
- Use `MiddlewareHelpers.SafeDispatchAsync(IDispatchContext, IAction)` to dispatch from background work safely.
- Consider enabling `StoreOptions.DebugGuards` during development to detect blocking awaits or sync waits inside
  dispatch.
- Example (safe): schedule background work and call `SafeDispatchAsync` after yields.
