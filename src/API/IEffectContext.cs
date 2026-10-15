namespace StatePie
{
    /// <summary>
    /// Gives an effect the store-scoped cancellation token and a safe dispatch path for result actions, so asynchronous work never needs a reference to the store itself.
    /// </summary>
    public interface IEffectContext<TAction> where TAction : class, IAction
    {
        /// <summary>
        /// Gets a token that is cancelled when the store is disposed, allowing in-flight work to stop.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Dispatches a result action through the full middleware and reducer pipeline; results arriving after the store is disposed are dropped instead of throwing.
        /// </summary>
        /// <param name="action">The result action to dispatch.</param>
        void Dispatch(TAction action);
    }
}
