using AtomicState;

namespace SC.AtomicState.Dev
{
    public class Programs
    {
        public static void Main()
        {
            const string counterIncrement = "@COUNTER/INCREMENT";
            const string counterDecrement = "@COUNTER/DECREMENT";
            const string profileSetName = "@PROFILE/SET_NAME";

            var counterSlice = StateFactory.CreateSlice(
                name: "counter",
                initialState: new CounterState { Count = 0 },
                reducer: (state, action) => action.Type switch
                {
                    counterIncrement => new CounterState { Count = state.Count + 1 },
                    counterDecrement => new CounterState { Count = state.Count - 1 },
                    _ => state
                });

            var profileSlice = StateFactory.CreateSlice(
                name: "profile",
                initialState: new ProfileState { Name = "Guest" },
                reducer: (state, action) => action.Type switch
                {
                    profileSetName when action is IAction<string> nameAction
                        => new ProfileState { Name = nameAction.Payload },
                    _ => state
                });

            using var store = StateFactory.CreateStore(counterSlice, profileSlice);

            store.OnStateChanged += PrintState;

            Console.WriteLine("Initial:");
            PrintState(store.State);

            store.Dispatch(StateFactory.CreateAction(counterIncrement));
            store.Dispatch(StateFactory.CreateAction(counterIncrement));
            store.Dispatch(StateFactory.CreateAction(profileSetName, "Ada"));
            store.Dispatch(StateFactory.CreateAction(counterDecrement));
        }

        private static void PrintState(IReadOnlyDictionary<string, IState> rootState)
        {
            var counter = (CounterState)rootState["counter"];
            var profile = (ProfileState)rootState["profile"];

            Console.WriteLine($"Counter={counter.Count}, Name={profile.Name}");
        }

        private sealed class CounterState : IState
        {
            public int Count { get; init; }
        }

        private sealed class ProfileState : IState
        {
            public string Name { get; init; } = string.Empty;
        }
    }
}
