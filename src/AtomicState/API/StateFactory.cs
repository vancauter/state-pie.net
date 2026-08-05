
using System.Collections.ObjectModel;

namespace AtomicState
{
    public static class StateFactory
    {
        public static IAction CreateAction(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));

            return new Action(type);
        }

        public static IAction<T> CreateAction<T>(string type, T payload) where T : notnull
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));

            return new Action<T>(type, payload);
        }

        public static ISlice CreateSlice<TState>(string name, TState initialState, Reducer<TState> reducer) where TState : IState
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Slice name must be a non-empty string.", nameof(name));

            if (initialState is null)
                throw new ArgumentNullException(nameof(initialState));

            if (reducer is null)
                throw new ArgumentNullException(nameof(reducer));

            return new Slice<TState>(name, initialState, reducer);
        }

        public static IStore CreateStore(params ISlice[] slices)
        {
            if (slices is null || slices.Length == 0)
                throw new ArgumentException("Must provide at least one slice to create a store.", nameof(slices));

            if (slices.Any(static slice => slice is null))
                throw new ArgumentException("Slices cannot contain null entries.", nameof(slices));

            var reducerDictionary = new Dictionary<string, Reducer>(StringComparer.Ordinal);
            var initialSliceStates = new Dictionary<string, IState>(StringComparer.Ordinal);

            foreach (var slice in slices)
            {
                if (reducerDictionary.ContainsKey(slice.Name))
                    throw new ArgumentException($"Duplicate slice name detected: {slice.Name}", nameof(slices));

                reducerDictionary.Add(slice.Name, slice.Reducer);
                initialSliceStates.Add(slice.Name, slice.InitialState);
            }

            var initialRootState = new ReadOnlyDictionary<string, IState>(initialSliceStates);

            IReadOnlyDictionary<string, IState> rootReducerCore(IReadOnlyDictionary<string, IState> state, IAction action)
            {
                Dictionary<string, IState> nextSliceStates = null;

                foreach (var pair in reducerDictionary)
                {
                    if (!state.TryGetValue(pair.Key, out var currentSliceState))
                        throw new InvalidOperationException($"Missing slice state for key '{pair.Key}'.");

                    var nextSliceState = pair.Value(currentSliceState, action);
                    if (ReferenceEquals(nextSliceState, currentSliceState))
                        continue;

                    nextSliceStates ??= new Dictionary<string, IState>(StringComparer.Ordinal);
                    if (nextSliceStates.Count == 0)
                    {
                        foreach (var currentPair in state)
                            nextSliceStates.Add(currentPair.Key, currentPair.Value);
                    }

                    nextSliceStates[pair.Key] = nextSliceState;
                }

                return nextSliceStates is null ? state : new ReadOnlyDictionary<string, IState>(nextSliceStates);
            }

            return new Store(initialRootState, rootReducerCore);
        }
    }
}