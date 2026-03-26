using System.Collections.Immutable;

namespace SC.AtomicState.MiddlewareEx
{
    public interface ITimeTravelController
    {
        Task UndoAsync();
        Task RedoAsync();
        Task ReplayAsync(IEnumerable<IAction> actions);
        IReadOnlyList<IState> GetHistory();
        void Clear();
    }

    internal sealed class TimeTravelController(Func<IState, Task> applyStateAsync) : ITimeTravelController
    {
        private readonly List<IState> _history = [];
        private readonly List<IAction> _actions = [];
        private int _cursor = -1;
        private readonly Func<IState, Task> _applyStateAsync = applyStateAsync;

        public Task UndoAsync()
        {
            if (_cursor <= 0) return Task.CompletedTask;
            _cursor--;
            return _applyStateAsync(_history[_cursor]);
        }

        public Task RedoAsync()
        {
            if (_cursor >= _history.Count - 1) return Task.CompletedTask;
            _cursor++;
            return _applyStateAsync(_history[_cursor]);
        }

        public Task ReplayAsync(IEnumerable<IAction> actions) =>
            // Replay is implemented by consumers via CreateSeededStore; here we simply expose the action list.
            Task.CompletedTask;

        public IReadOnlyList<IState> GetHistory() => _history.ToImmutableArray();

        public void Clear()
        {
            _history.Clear();
            _actions.Clear();
            _cursor = -1;
        }

        internal void Record(IState state, IAction? action)
        {
            _history.Add(state);
            if (action is not null) _actions.Add(action);
            _cursor = _history.Count - 1;
        }
    }
}
