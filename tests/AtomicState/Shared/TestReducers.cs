namespace SC.AtomicState.Tests.Shared
{
    /// <summary>
    /// Centralizes small reducer shapes that multiple tests reuse so behavior-focused tests do not repeat incidental setup.
    /// </summary>
    internal static class TestReducers
    {
        /// <summary>
        /// Returns the existing state instance for actions the test intends to ignore.
        /// </summary>
        internal static IState NoOp(IState state, IAction _) => state;

        /// <summary>
        /// Increments <see cref="CounterState.Count"/> when the counter increment action is dispatched.
        /// </summary>
        internal static IState CounterIncrement(IState state, IAction action)
        {
            var currentState = (CounterState)state;

            return action.Type == TestSlices.Increment
                ? new CounterState(currentState.Count + 1)
                : state;
        }
    }
}