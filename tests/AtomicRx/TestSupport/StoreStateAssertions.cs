using System.Reflection;

namespace AtomicRx.Tests.TestSupport
{
    internal static class StoreStateAssertions
    {
        public static TState GetSliceState<TState>(IState rootState, string sliceName)
            where TState : class, IState
        {
            ArgumentNullException.ThrowIfNull(rootState);
            ArgumentException.ThrowIfNullOrWhiteSpace(sliceName);

            var statesProperty = rootState.GetType().GetProperty(
                "States",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var states = Assert.IsAssignableFrom<IReadOnlyDictionary<string, IState>>(statesProperty?.GetValue(rootState));
            Assert.True(states.TryGetValue(sliceName, out var sliceState), $"Expected slice '{sliceName}' to exist.");

            return Assert.IsType<TState>(sliceState);
        }
    }
}