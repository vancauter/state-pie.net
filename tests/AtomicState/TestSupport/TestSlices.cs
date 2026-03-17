namespace SC.AtomicState.Tests.TestSupport
{
    public sealed record CounterState(int Count) : IState;
    public sealed record PersonState(string Name, int Age) : IState;

    public static class TestSlices
    {
        public const string Increment = "counter.increment";
        public const string Decrement = "counter.decrement";
        public const string InnerIncrement = "inner.counter.increment";
        public const string InnerDecrement = "inner.counter.decrement";
        public const string InnerPersonAddAge = "inner.person.add_age";
        public const string InnerPersonSetName = "inner.person.set_name";
        public const string PersonAddAge = "person.add_age";
        public const string PersonSetName = "person.set_name";

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

        internal static ISlice CreateInnerPersonSlice() => Factory.CreateSlice(
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

        internal static ISlice CreateNestedSlice()
        {
            var (innerState, innerReducer) = Factory.CombineReducers(CreateInnerCounterSlice(), CreateInnerPersonSlice());

            IState Adapter(IState state, IAction action) => innerReducer(state, action);

            return Factory.CreateSlice(
                name: "nested",
                initialState: innerState,
                reducer: Adapter);
        }

        public static ISlice CreatePersonSlice() => Factory.CreateSlice(
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