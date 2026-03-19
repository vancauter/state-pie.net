

namespace SC.AtomicState.Internal
{
    /// <summary>
    /// Implements the store that owns serialized dispatch, state publication, and observable notifications.
    /// </summary>
    internal sealed class Store : IStore
    {
        private static readonly IAction s_initAction = Factory.CreateAction("@@INIT");
        private readonly Lock _dispatchLock = new();
        private readonly Dispatch _dispatch;
        private readonly StateStream _stateStream;
        private IState _state;
        private bool _isDisposed;
        private readonly Reducer _rootReducer;

        /// <summary>
        /// Initializes a new store.
        /// </summary>
        /// <param name="initialState">The initial root state to expose before the initialization pass runs.</param>
        /// <param name="rootReducer">The root reducer that computes committed state transitions.</param>
        /// <param name="middleware">Optional middleware applied around the serialized dispatch core.</param>
        internal Store(IState initialState, Reducer rootReducer, params Middleware[] middleware)
        {
            _rootReducer = rootReducer;
            _stateStream = new StateStream(initialState);
            _state = initialState;

            var middlewareContext = new MiddlewareDispatchContext(this);

            if (middleware.Length > 0)
            {
                var current = (Dispatch)DispatchCore;
                for (var index = middleware.Length - 1; index >= 0; index--)
                {
                    // Compose from the tail so the first registered middleware becomes the outermost dispatch layer.
                    current = middleware[index](middlewareContext, current);
                }

                _dispatch = current;
            }
            else
            {
                _dispatch = DispatchCore;
            }

            RunInitPass();
        }

        /// <inheritdoc />
        public IObservable<IState> StateChanged => _stateStream;

        /// <inheritdoc />
        public IState State => Volatile.Read(ref _state);

        /// <inheritdoc />
        public void Dispatch(IAction action) => _dispatch(action);

        /// <summary>
        /// Disposes the store and completes the state stream.
        /// </summary>
        /// <remarks>
        /// Disposal is idempotent. The disposed transition is serialized with dispatch so no later reducer execution can
        /// commit additional state after disposal wins.
        /// </remarks>
        public void Dispose()
        {
            lock (_dispatchLock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;
            }

            _stateStream.Dispose();
        }

        /// <summary>
        /// Runs the active root reducer and publishes changed state.
        /// </summary>
        private void DispatchCore(IAction action)
        {
            IState? stateToPublish = null;

            lock (_dispatchLock)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, nameof(Store));

                var currentState = _state;
                var nextState = _rootReducer(currentState, action);
                if (!ReferenceEquals(nextState, currentState))
                {
                    // Publish with release semantics before notifying observers so lock-free readers can observe the
                    // same committed state that the stream is about to emit.
                    Volatile.Write(ref _state, nextState);
                    stateToPublish = nextState;
                }
            }

            // Publish outside the reducer lock so observer callbacks cannot block future dispatches.
            if (stateToPublish is not null)
            {
                _stateStream.OnNext(stateToPublish);
            }
        }

        /// <summary>
        /// Dispatches the internal initialization action.
        /// </summary>
        /// <remarks>
        /// Initialization is part of successful store creation, so reducer or middleware failures are propagated to
        /// the caller instead of leaving a partially initialized store instance in circulation.
        /// </remarks>
        private void RunInitPass() => _dispatch(s_initAction);

        /// <summary>
        /// Exposes only dispatch to middleware so the extension boundary matches the supported runtime behavior.
        /// </summary>
        private sealed class MiddlewareDispatchContext(Store owner) : IDispatchContext
        {
            private readonly Store _owner = owner;

            public void Dispatch(IAction action)
                => _owner.Dispatch(action);
        }
    }
}