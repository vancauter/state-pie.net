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

    internal sealed class TimeTravelController : ITimeTravelController
    {
        private readonly List<IState> _history = new();
        private readonly List<IAction> _actions = new();
        private int _cursor = -1;
        private readonly TimeTravelOptions _options;
        private readonly Func<TimeTravelOptions, IState, Task> _applyStateAsync;
        private bool _suppressRecording;

        public TimeTravelController(TimeTravelOptions options, Func<IState, Task> applyStateAsync)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(applyStateAsync);

            _options = options;
            _applyStateAsync = (_, s) => applyStateAsync(s);
        }

        public async Task UndoAsync()
        {
            if (_cursor <= 0) return;
            _cursor--;
            try
            {
                _suppressRecording = true;
                await _applyStateAsync(_options, _history[_cursor]).ConfigureAwait(false);
            }
            finally
            {
                _suppressRecording = false;
            }
        }

        public async Task RedoAsync()
        {
            if (_cursor >= _history.Count - 1) return;
            _cursor++;
            try
            {
                _suppressRecording = true;
                await _applyStateAsync(_options, _history[_cursor]).ConfigureAwait(false);
            }
            finally
            {
                _suppressRecording = false;
            }
        }

        public async Task ReplayAsync(IEnumerable<IAction> actions)
        {
            if (actions is null) return;

            // Try to locate the provided action sequence in the recorded actions and, if found,
            // apply the corresponding history state that follows that sequence. If not found, no-op.
            var actionList = actions.ToList();
            if (actionList.Count == 0) return;

            // Build a simple search for a contiguous matching subsequence in recorded actions.
            for (var start = 0; start + actionList.Count - 1 < _actions.Count; start++)
            {
                var match = true;
                for (var i = 0; i < actionList.Count; i++)
                {
                    if (!ReferenceEquals(_actions[start + i], actionList[i]) && _actions[start + i].Type != actionList[i].Type)
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    var targetIndex = start + actionList.Count - 1;
                    if (targetIndex >= 0 && targetIndex < _history.Count)
                    {
                        try
                        {
                            _suppressRecording = true;
                            await _applyStateAsync(_options, _history[targetIndex]).ConfigureAwait(false);
                        }
                        finally
                        {
                            _suppressRecording = false;
                        }

                        return;
                    }
                }
            }
        }

        public IReadOnlyList<IState> GetHistory() => _history.ToImmutableArray();

        public void Clear()
        {
            _history.Clear();
            _actions.Clear();
            _cursor = -1;
        }

        internal void Record(IState state, IAction? action)
        {
            if (_suppressRecording) return;

            _history.Add(state);
            if (action is not null && _options.StoreActions) _actions.Add(action);
            _cursor = _history.Count - 1;

            // Enforce MaxHistory eviction
            if (_options.MaxHistory > 0 && _history.Count > _options.MaxHistory)
            {
                var excess = _history.Count - _options.MaxHistory;
                _history.RemoveRange(0, excess);
                if (_actions.Count >= excess) _actions.RemoveRange(0, Math.Min(excess, _actions.Count));
                _cursor = _history.Count - 1;
            }
        }
    }
}
