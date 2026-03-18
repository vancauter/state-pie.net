using System.Diagnostics.CodeAnalysis;
using SC.AtomicState.Internal;

namespace SC.AtomicState
{
    /// <summary>
    /// Provides supported access to slice state produced by <see cref="Factory.CombineReducers(ISlice[])"/>.
    /// </summary>
    public static class StateAccess
    {
        /// <summary>
        /// Gets a named slice state from a combined root state and casts it to <typeparamref name="TState"/>.
        /// </summary>
        /// <typeparam name="TState">Expected slice state type.</typeparam>
        /// <param name="rootState">Root state produced by <see cref="Factory.CombineReducers(ISlice[])"/>.</param>
        /// <param name="sliceName">The slice name to read.</param>
        /// <returns>The typed slice state.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sliceName"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="rootState"/> was not produced by <see cref="Factory.CombineReducers(ISlice[])"/>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the combined root state does not contain <paramref name="sliceName"/>.</exception>
        /// <exception cref="InvalidCastException">Thrown when the slice exists but is not assignable to <typeparamref name="TState"/>.</exception>
        public static TState GetSliceState<TState>(IState rootState, string sliceName)
            where TState : class, IState
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sliceName);

            if (rootState is not CombinedState combinedState)
            {
                throw new InvalidOperationException(
                    $"Root state '{rootState.GetType().Name}' was not created by {nameof(Factory)}.{nameof(Factory.CombineReducers)}.");
            }

            if (!combinedState.States.TryGetValue(sliceName, out var sliceState))
            {
                throw new KeyNotFoundException($"Combined root state does not contain a slice named '{sliceName}'.");
            }

            if (sliceState is not TState typedState)
            {
                throw new InvalidCastException(
                    $"Slice '{sliceName}' is '{sliceState.GetType().Name}', not '{typeof(TState).Name}'.");
            }

            return typedState;
        }

        /// <summary>
        /// Attempts to get a named slice state from a combined root state.
        /// </summary>
        /// <typeparam name="TState">Expected slice state type.</typeparam>
        /// <param name="rootState">Root state produced by <see cref="Factory.CombineReducers(ISlice[])"/>.</param>
        /// <param name="sliceName">The slice name to read.</param>
        /// <param name="sliceState">The typed slice state when the lookup succeeds; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when the slice exists and is assignable to <typeparamref name="TState"/>; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="rootState"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sliceName"/> is <see langword="null"/>, empty, or whitespace.</exception>
        public static bool TryGetSliceState<TState>(IState rootState, string sliceName, [NotNullWhen(true)] out TState? sliceState)
            where TState : class, IState
        {
            ArgumentNullException.ThrowIfNull(rootState);
            ArgumentException.ThrowIfNullOrWhiteSpace(sliceName);

            if (rootState is CombinedState combinedState &&
                combinedState.States.TryGetValue(sliceName, out var candidate) &&
                candidate is TState typedState)
            {
                sliceState = typedState;
                return true;
            }

            sliceState = null;
            return false;
        }
    }
}