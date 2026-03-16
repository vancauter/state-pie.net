namespace AtomicRx.Tests.TestStore
{
    public sealed record CounterState(int Count) : IState;
    public sealed record PersonState(string Name, int Age) : IState;

    public static class Slices
    {

        #region Counter slice

        public const string Increment = "counter.increment";
        public const string Decrement = "counter.decrement";

        internal static ISlice CreateCounterSlice() => Factory.CreateSlice(
            name: "counter",
            initialState: new CounterState(0),
            reducer: (state, action) =>
            {
                return action.Type switch
                {
                    Increment => new CounterState(state.Count + 1),
                    Decrement => new CounterState(state.Count - 1),
                    _ => state
                };
            });

        #endregion

        #region Nested (inner) slices

        // Inner counter slice used as part of a nested CombinedState. Handles its own
        // increment/decrement actions so we can exercise nested reducer behavior.
        public const string InnerIncrement = "inner.counter.increment";
        public const string InnerDecrement = "inner.counter.decrement";

        internal static ISlice CreateInnerCounterSlice() => Factory.CreateSlice(
            name: "innerCounter",
            initialState: new CounterState(5),
            reducer: (state, action) =>
            {
                return action.Type switch
                {
                    InnerIncrement => new CounterState(state.Count + 1),
                    InnerDecrement => new CounterState(state.Count - 1),
                    _ => state
                };
            });

        // Inner person slice used as part of a nested CombinedState. Supports
        // add-age and set-name actions with an optional typed payload.
        public const string InnerPersonAddAge = "inner.person.add_age";
        public const string InnerPersonSetName = "inner.person.set_name";

        internal static ISlice CreateInnerPersonSlice() => Factory.CreateSlice(
            name: "innerPerson",
            initialState: new PersonState("Inner", 10),
            reducer: (state, action) =>
            {
                return action.Type switch
                {
                    InnerPersonAddAge => new PersonState(state.Name, state.Age + 1),
                    InnerPersonSetName when action is IAction<string> a => new PersonState(a.Payload, state.Age),
                    _ => state
                };
            });

        // Create a slice whose state is itself a CombinedState (nested combined state).
        // This lets us exercise serialization and nested reducer behavior.
        internal static ISlice CreateNestedSlice()
        {
            var (innerState, innerReducer) = Factory.CombineReducers(CreateInnerCounterSlice(), CreateInnerPersonSlice());

            // Adapter to match the generic Reducer<TState> expected by CreateSlice<TState>
            IState adapter(IState s, IAction a) => innerReducer(s, a);

            return Factory.CreateSlice(
                name: "nested",
                initialState: innerState,
                reducer: adapter
            );
        }

        #endregion

        #region Person slice

        public const string PersonAddAge = "person.add_age";
        public const string PersonSetName = "person.set_name";

        public static ISlice CreatePersonSlice() => Factory.CreateSlice(
            name: "person",
            initialState: new PersonState("Guest", 30),
            reducer: (state, action) =>
            {
                return action.Type switch
                {
                    PersonAddAge => new PersonState(state.Name, state.Age + 1),
                    PersonSetName when action is IAction<string> a => new PersonState(a.Payload, state.Age),
                    _ => state
                };
            });

        #endregion
    }
}
