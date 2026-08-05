using System.Collections.Generic;

namespace AtomicState
{
    public interface IStore : System.IDisposable
    {
        IReadOnlyDictionary<string, IState> State { get; }

        event System.Action<IReadOnlyDictionary<string, IState>> OnStateChanged;

        void Dispatch(IAction action);
    }
}