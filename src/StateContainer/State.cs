using System.Collections.ObjectModel;

namespace StateContainer
{
    // `IState` is declared in Interfaces.cs; concrete implementations are internal

    internal sealed class State : BaseClass, IState;

    public class CombinedState : BaseClass, IState
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