using AtomicRx.Core;
using System.Collections.ObjectModel;

namespace AtomicRx
{
    public static partial class Factory
    {
        /// <summary>
        /// Combine multiple <see cref="ISlice"/> instances into a single root <see cref="IState"/>
        /// and a <see cref="Reducer"/> that delegates to each slice reducer.
        /// </summary>
        /// <param name="slices">An array of slices to combine. Each slice must have a unique name.</param>
        /// <returns>
        /// A tuple containing the initial combined root state and a root reducer function that
        /// forwards actions to each slice reducer and returns an updated combined state when any slice changes.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when no slices are provided or duplicate slice names are detected.</exception>
        public static (IState state, Reducer reducer) CombineReducers(params ISlice[] slices)
        {
            if (slices.Length == 0)
                throw new ArgumentException("Must provide at least one slice to combine.", nameof(slices));

            // detect duplicate slice names early for clearer error messages
            var duplicateGroup = slices.GroupBy(s => s.Name).FirstOrDefault(g => g.Count() > 1);
            if (duplicateGroup != null)
                throw new ArgumentException($"Duplicate slice name detected: {duplicateGroup.Key}", nameof(slices));

            var reducerDictionary = slices.ToDictionary(slice => slice.Name, slice => slice.Reducer);
            // Build a mutable dictionary first, then wrap in ReadOnlyDictionary to avoid copying inside Create.
            var mutableSliceStates = slices.ToDictionary(slice => slice.Name, slice => slice.State);
            var initialSliceStates = new ReadOnlyDictionary<string, IState>(mutableSliceStates);
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