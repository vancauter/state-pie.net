using System.Collections.ObjectModel;
using SC.AtomicState.Runtime;

namespace SC.AtomicState
{
    /// <summary>
    /// Provides factory helpers for constructing actions, slices, reducers, and stores.
    /// </summary>
    public static partial class Factory
    {
        /// <summary>
        /// Combines multiple <see cref="ISlice"/> instances into a single root state and reducer.
        /// </summary>
        /// <param name="slices">An array of slices to combine. Each slice must have a unique name.</param>
        /// <returns>A tuple containing the initial combined root state and a reducer that delegates to each slice reducer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="slices"/> is empty, contains a null entry, or contains duplicate slice names.</exception>
        /// <remarks>
        /// The returned reducer expects to receive the root state shape created by this method. It preserves the existing root state reference when no slice produces a new state instance.
        /// </remarks>
        public static (IState state, Reducer reducer) CombineReducers(params ISlice[] slices)
        {
            if (slices.Length == 0)
            {
                throw new ArgumentException("Must provide at least one slice to combine.", nameof(slices));
            }

            if (slices.Any(static slice => slice is null))
            {
                throw new ArgumentException("Slices cannot contain null entries.", nameof(slices));
            }

            var duplicateGroup = slices.GroupBy(slice => slice.Name, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicateGroup is not null)
            {
                throw new ArgumentException($"Duplicate slice name detected: {duplicateGroup.Key}", nameof(slices));
            }

            var reducerDictionary = slices.ToDictionary(slice => slice.Name, slice => slice.Reducer, StringComparer.Ordinal);
            var mutableSliceStates = slices.ToDictionary(slice => slice.Name, slice => slice.State, StringComparer.Ordinal);
            var initialSliceStates = new ReadOnlyDictionary<string, IState>(mutableSliceStates);
            var initialRootState = new CombinedState(initialSliceStates);

            IState rootReducer(IState state, IAction action)
            {
                if (state is not CombinedState currentRootState)
                {
                    throw new InvalidOperationException(
                        $"[CombineReducers] Unexpected state type: {state?.GetType().Name}. Expected {nameof(CombinedState)}.");
                }

                // Clone once so each slice can update independently while unchanged slices retain identity.
                var nextSliceStates = new Dictionary<string, IState>(currentRootState.States, StringComparer.Ordinal);
                var changed = false;

                foreach (var sliceEntry in reducerDictionary)
                {
                    if (!currentRootState.States.TryGetValue(sliceEntry.Key, out var currentSliceState))
                    {
                        throw new InvalidOperationException(
                            $"[CombineReducers] Missing slice state for key: {sliceEntry.Key} in current root state.");
                    }

                    var nextSliceState = sliceEntry.Value(currentSliceState, action);
                    if (!ReferenceEquals(nextSliceState, currentSliceState))
                    {
                        nextSliceStates[sliceEntry.Key] = nextSliceState;
                        changed = true;
                    }
                }

                return changed ? new CombinedState(nextSliceStates) : currentRootState;
            }

            return (initialRootState, rootReducer);
        }
    }
}