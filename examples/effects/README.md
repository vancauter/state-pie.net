# Effects

Shows asynchronous work started outside reducers. The effect starts after `LoadProfileAction` is committed, awaits a
simulated lookup, and returns its result by dispatching a typed action through the effect context. Expected failures
become a failure action; the store's error handler only receives unexpected faults.

- Reducers stay pure and synchronous.
- Dispatch does not wait for effects; results arrive as ordinary dispatches.
- The effect's cancellation token is triggered when the store is disposed.
