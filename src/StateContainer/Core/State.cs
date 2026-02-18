using System.Collections.ObjectModel;

namespace SCLibs.StateContainer
{
    // `IState` is declared in Interfaces.cs; concrete implementations are internal
    internal sealed class State : IState
    {
        // This class intentionally left blank as a marker for valid state types. Concrete state implementations can be defined by users of the library as needed.
    };

    internal class CombinedState : IState
    {
        public IReadOnlyDictionary<string, IState> States { get; }

        internal CombinedState(IReadOnlyDictionary<string, IState> sliceStates) => States = sliceStates;

        internal static CombinedState Create(IDictionary<string, IState> sliceStates)
        {
            // create a defensive copy to avoid external mutation of the provided dictionary
            var copied = new Dictionary<string, IState>(sliceStates);
            var newSliceStates = new ReadOnlyDictionary<string, IState>(copied);
            return new CombinedState(newSliceStates);
        }
    }
}