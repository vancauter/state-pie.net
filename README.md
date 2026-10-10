# Snapshot Store

Snapshot Store is a lightweight C# state management library for predictable, immutable application state updates using
actions, reducers, and slices.

## Simple Example

```csharp
using SnapshotStore;

public sealed class CounterState : IState
{
	public int Count { get; init; }
}

var counterSlice = SnapshotStoreFactory.CreateSlice(
	name: "counter",
	initialState: new CounterState { Count = 0 },
	reducer: static (state, action) =>
	{
		if (action is IAction<int> typed && action.Type == "counter/set")
			return new CounterState { Count = typed.Payload };

		if (action.Type == "counter/increment")
			return new CounterState { Count = state.Count + 1 };

		return state;
	});

using var store = SnapshotStoreFactory.CreateStore(counterSlice);

store.OnStateChanged += state =>
{
	var counter = (CounterState)state["counter"];
	Console.WriteLine($"Count: {counter.Count}");
};

store.Dispatch(SnapshotStoreFactory.CreateAction("counter/increment"));
store.Dispatch(SnapshotStoreFactory.CreateAction("counter/set", 10));
```
