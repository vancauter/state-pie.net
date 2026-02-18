namespace AtomicRx.Core
{
    internal sealed class State : IState
    {
        // Marker type for simple/placeholder states. Left blank intentionally.
    };

    // Internal implementation of a combined state that holds the current state of each slice in a dictionary.
    // This type is used as the root state when combining multiple slices. Reducers that combine slices are expected
    // to return an instance of this type containing the updated slice states.
    internal sealed class CombinedState : IState
    {
        internal IReadOnlyDictionary<string, IState> States { get; }

        internal CombinedState(IReadOnlyDictionary<string, IState> sliceStates) => States = sliceStates;

        // Accept an IReadOnlyDictionary to allow callers to provide an already-readonly wrapper
        // and avoid copying the underlying entries. This method is internal and currently only
        // used by Factory.CombineReducers which wraps the mutable dictionary in a ReadOnlyDictionary
        // before calling Create.
        internal static CombinedState Create(IReadOnlyDictionary<string, IState> sliceStates) => new(sliceStates);
    }
}