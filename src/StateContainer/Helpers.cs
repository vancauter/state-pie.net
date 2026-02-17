namespace StateContainer
{
    public static partial class Helpers
    {
        // Actions
        public static IAction CreateAction(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));

            return new StoreAction(type);
        }

        public static IAction CreateActionPayload<TPayload>(string type, TPayload payload)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));

            return new StoreAction<TPayload>(type, payload);
        }

        // CreateSlice
        public static Slice<TState> CreateSlice<TState>(string name, TState initialState, Func<TState, IAction, TState> reducerFn) where TState : IState
            => new(name, initialState, reducerFn);

        // CombineReducers
        public static (CombinedState initialState, Reducer rootReducer) CombineReducers(params ISlice[] slices)
        {
            if (slices == null || slices.Length == 0)
                throw new ArgumentException("Must provide at least one slice to combine.", nameof(slices));

            // detect duplicate slice names early for clearer error messages
            var duplicateGroup = slices.GroupBy(s => s.Name).FirstOrDefault(g => g.Count() > 1);
            if (duplicateGroup != null)
                throw new ArgumentException($"Duplicate slice name detected: {duplicateGroup.Key}", nameof(slices));

            var reducerDictionary = slices.ToDictionary(slice => slice.Name, slice => slice.ReducerFn);
            var initialSliceStates = slices.ToDictionary(slice => slice.Name, slice => slice.InitialState);
            var initialRootState = CombinedState.Create(initialSliceStates);

            IState rootReducerDelegate(IState state, IAction action)
            {
                if (state is not CombinedState currentRootState)
                {
                    throw new InvalidOperationException(
                        $"[CombineReducers] Unexpected state type: {state?.GetType().Name}. Expected {nameof(CombinedState)}."
                    );
                }
                var nextSliceStates = new Dictionary<string, IState>(currentRootState.States);
                var changed = false;

                foreach (var entity in reducerDictionary)
                {
                    var sliceName = entity.Key;
                    var sliceReducer = entity.Value;

                    if (!currentRootState.States.TryGetValue(sliceName, out var currentSliceState))
                    {
                        throw new InvalidOperationException(
                            $"[CombineReducers] Missing slice state for key: {sliceName} in current root state."
                        );
                    }

                    var nextSliceState = sliceReducer(currentSliceState, action);

                    // use reference-equality to detect changes consistently with the store
                    if (!ReferenceEquals(nextSliceState, currentSliceState))
                    {
                        nextSliceStates[sliceName] = nextSliceState;
                        changed = true;
                    }
                }

                return changed ? CombinedState.Create(nextSliceStates) : currentRootState;
            }

            return (initialRootState, rootReducerDelegate);
        }
    }
}
