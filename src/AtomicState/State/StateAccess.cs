using System.Diagnostics.CodeAnalysis;
using SC.AtomicState.Internal;

namespace SC.AtomicState
{
    /// <summary>
    /// Provides supported access to slice state produced by <see cref="Factory.CombineReducers(SliceHandle[])"/>.
    /// </summary>
    public static class StateAccess
    {
        /// <summary>
        /// Gets a typed slice state from a combined root state using a previously created slice handle.
        /// </summary>
        /// <typeparam name="TState">Expected slice state type.</typeparam>
        /// <param name="rootState">A combined root state produced by <see cref="Factory.CombineReducers(SliceHandle[])"/>.</param>
        /// <param name="sliceHandle">The slice handle to read.</param>
        /// <returns>The typed slice state.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="rootState"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sliceHandle"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="rootState"/> is not a combined state produced by <see cref="Factory.CombineReducers(SliceHandle[])"/>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the combined root state does not contain the requested slice.</exception>
        /// <exception cref="InvalidCastException">Thrown when the slice exists but is not assignable to <typeparamref name="TState"/>.</exception>
        /// <remarks>
        /// Use <see cref="TryGetSliceState{TState}(IState, SliceHandle{TState}, out TState?)"/> when the caller wants an optional
        /// lookup instead of exception-based failure details.
        /// </remarks>
        public static TState GetSliceState<TState>(IState rootState, SliceHandle<TState> sliceHandle)
            where TState : class, IState
        {
            ArgumentNullException.ThrowIfNull(rootState);
            ArgumentNullException.ThrowIfNull(sliceHandle);

            if (rootState is not CombinedState combinedState)
                throw new InvalidOperationException($"Root state '{rootState.GetType().Name}' was not created by {nameof(Factory)}.{nameof(Factory.CombineReducers)}.");

            if (!combinedState.States.TryGetValue(sliceHandle.Name, out var sliceState))
                throw new KeyNotFoundException($"Combined root state does not contain a slice named '{sliceHandle.Name}'.");

            if (sliceState is not TState typedState)
                throw new InvalidCastException($"Slice '{sliceHandle.Name}' is '{sliceState.GetType().Name}', not '{typeof(TState).Name}'.");

            return typedState;
        }

        /// <summary>
        /// Attempts to get a named slice state from a combined root state.
        /// </summary>
        /// <typeparam name="TState">Expected slice state type.</typeparam>
        /// <param name="rootState">Root state produced by <see cref="Factory.CombineReducers(SliceHandle[])"/>.</param>
        /// <param name="sliceHandle">The slice handle to read.</param>
        /// <param name="sliceState">The typed slice state when the lookup succeeds; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when the slice exists and is assignable to <typeparamref name="TState"/>; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="rootState"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sliceHandle"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method intentionally folds unsupported root-state shapes, missing slices, and incompatible slice types
        /// into the same <see langword="false"/> result.
        /// </remarks>
        public static bool TryGetSliceState<TState>(IState rootState, SliceHandle<TState> sliceHandle, [NotNullWhen(true)] out TState? sliceState)
            where TState : class, IState
        {
            ArgumentNullException.ThrowIfNull(rootState);
            ArgumentNullException.ThrowIfNull(sliceHandle);

            if (rootState is CombinedState combinedState &&
                combinedState.States.TryGetValue(sliceHandle.Name, out var candidate) &&
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