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
