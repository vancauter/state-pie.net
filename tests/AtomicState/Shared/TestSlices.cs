namespace SC.AtomicState.Tests.Shared
{
    /// <summary>
    /// Minimal counter state used by reducer and store tests that only care about numeric transitions.
    /// </summary>
    internal sealed record CounterState(int Count) : IState;

    /// <summary>
    /// Minimal person state used by combined-state tests that need a second independently addressable slice.
    /// </summary>
    internal sealed record PersonState(string Name, int Age) : IState;

    /// <summary>
    /// Provides reusable action names and slice factories so tests can share the same lightweight state model.
    /// </summary>
    internal static class TestSlices
    {
        internal const string Increment = "counter.increment";
        internal const string Decrement = "counter.decrement";
        internal const string InnerIncrement = "inner.counter.increment";
        internal const string InnerDecrement = "inner.counter.decrement";
        internal const string InnerPersonAddAge = "inner.person.add_age";
        internal const string InnerPersonSetName = "inner.person.set_name";
        internal const string PersonAddAge = "person.add_age";
        internal const string PersonSetName = "person.set_name";

        internal static SliceHandle<CounterState> CreateCounterSlice() => Factory.CreateSlice(
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

        internal static SliceHandle<CounterState> CreateInnerCounterSlice() => Factory.CreateSlice(
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

        internal static SliceHandle<PersonState> CreateInnerPersonSlice() => Factory.CreateSlice(
            name: "innerPerson",
            initialState: new PersonState("Inner", 10),
            reducer: (state, action) =>
            {
                return action.Type switch
                {
                    InnerPersonAddAge => new PersonState(state.Name, state.Age + 1),
                    InnerPersonSetName when action is IAction<string> payloadAction => new PersonState(payloadAction.Payload, state.Age),
                    _ => state
                };
            });

        internal static SliceHandle<IState> CreateNestedSlice()
        {
            var (innerState, innerReducer) = Factory.CombineReducers(CreateInnerCounterSlice(), CreateInnerPersonSlice());

            // The adapter keeps the nested combined reducer usable through the untyped slice contract.
            IState adapter(IState state, IAction action) => innerReducer(state, action);

            return Factory.CreateSlice(
                name: "nested",
                initialState: innerState,
                reducer: adapter);
        }

        internal static SliceHandle<PersonState> CreatePersonSlice() => Factory.CreateSlice(
            name: "person",
            initialState: new PersonState("Guest", 30),
            reducer: (state, action) =>
            {
                return action.Type switch
                {
                    PersonAddAge => new PersonState(state.Name, state.Age + 1),
                    PersonSetName when action is IAction<string> payloadAction => new PersonState(payloadAction.Payload, state.Age),
                    _ => state
                };
            });
    }
}