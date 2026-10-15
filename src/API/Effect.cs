namespace StatePie
{
    /// <summary>
    /// Starts asynchronous work for a committed action and reports results by dispatching typed actions through the supplied context; effects start in registration order after reducers commit and subscribers are notified, are not awaited by dispatch, and never run for actions that middleware short-circuits or reducers reject.
    /// </summary>
    /// <typeparam name="TAction">The action family accepted by the store.</typeparam>
    /// <param name="action">The committed action that triggered this effect.</param>
    /// <param name="context">Provides the store-scoped cancellation token and the dispatch path for result actions.</param>
    /// <returns>A task representing the asynchronous work; faults are reported to the store's effect error handler.</returns>
    public delegate Task Effect<TAction>(TAction action, IEffectContext<TAction> context)
        where TAction : class, IAction;
}
