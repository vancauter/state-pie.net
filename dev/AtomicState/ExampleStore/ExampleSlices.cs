namespace SC.AtomicState.Dev.ExampleStore
{
    public sealed record CounterState(int Count) : IState;
    public sealed record PersonState(string Name, int Age) : IState;

    public static class ExampleSlices
    {
        public const string Increment = "@COUNTER/INCREMENT";
        public const string Decrement = "@COUNTER/DECREMENT";
        public const string PersonAddAge = "@PERSON/ADD_AGE";
        public const string PersonSetName = "@PERSON/SET_NAME";

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