namespace StatePie.Examples.MultipleSlices
{
    public interface IAppAction : IAction;

    public sealed record IncrementCounterAction : IAppAction;

    public sealed record AddTodoAction(string Text) : IAppAction;

    public sealed record CounterState(int Count) : IState;

    public sealed record TodosState(IReadOnlyList<string> Items) : IState;

    public static class Program
    {
        public static void Main()
        {
            var counterSlice = StoreFactory.CreateSlice<CounterState, IAppAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: static (state, action) => action switch
                {
                    IncrementCounterAction => state with { Count = state.Count + 1 },
                    _ => state
                });

            var todosSlice = StoreFactory.CreateSlice<TodosState, IAppAction>(
                name: "todos",
                initialState: new TodosState(Array.Empty<string>()),
                reducer: static (state, action) => action switch
                {
                    AddTodoAction add => state with { Items = state.Items.Append(add.Text).ToArray() },
                    _ => state
                });

            using var store = StoreFactory.CreateStore(counterSlice, todosSlice);

            using var subscription = store.Subscribe(static snapshot =>
            {
                var counter = (CounterState)snapshot["counter"];
                var todos = (TodosState)snapshot["todos"];

                Console.WriteLine($"Count: {counter.Count} | Todos: {todos.Items.Count}");
            });

            store.Dispatch(new IncrementCounterAction());
            store.Dispatch(new AddTodoAction("Review pull request"));
            store.Dispatch(new AddTodoAction("Ship release notes"));
        }
    }
}
