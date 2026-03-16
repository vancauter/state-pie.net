using AtomicRx.Core;
using System.Collections.ObjectModel;

namespace AtomicRx
{
    public static partial class Factory
    {
        /// <summary>
        /// Combines multiple <see cref="ISlice"/> instances into a single root <see cref="IState"/> and a coordinating <see cref="Reducer"/>.
        /// </summary>
        /// <param name="slices">An array of slices to combine. Each slice must have a unique name.</param>
        /// <returns>
        /// A tuple containing the initial combined root state and a root reducer that forwards each action to the slice reducers.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when no slices are provided or duplicate slice names are detected.</exception>
        /// <remarks>
        /// The returned reducer expects to receive a <see cref="CombinedState"/> created by this method. It preserves the existing root-state instance when no slice changes by reference.
        /// </remarks>
        public static (IState state, Reducer reducer) CombineReducers(params ISlice[] slices)
        {
            if (slices.Length == 0)
                throw new ArgumentException("Must provide at least one slice to combine.", nameof(slices));

            var duplicateGroup = slices.GroupBy(s => s.Name).FirstOrDefault(g => g.Count() > 1);
            if (duplicateGroup != null)
                throw new ArgumentException($"Duplicate slice name detected: {duplicateGroup.Key}", nameof(slices));

            var reducerDictionary = slices.ToDictionary(slice => slice.Name, slice => slice.Reducer);
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