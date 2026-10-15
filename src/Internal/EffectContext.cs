namespace StatePie
{
    /// <summary>
    /// Exposes only the cancellation token and result dispatch to effects, keeping store lifecycle control out of reach of asynchronous work.
    /// </summary>
    internal sealed class EffectContext<TAction> : IEffectContext<TAction> where TAction : class, IAction
    {
        private readonly Action<TAction> _dispatch;

        /// <summary>
        /// Creates a context that forwards result actions to the store-provided dispatch path.
        /// </summary>
        public EffectContext(CancellationToken cancellationToken, Action<TAction> dispatch)
        {
            CancellationToken = cancellationToken;
            _dispatch = dispatch;
        }

        public CancellationToken CancellationToken { get; }

        public void Dispatch(TAction action) => _dispatch(action);
    }
}
