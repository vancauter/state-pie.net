namespace SC.AtomicState
{
    /// <summary>
    /// Represents the store entry point that forwards an <see cref="IAction"/> into the dispatch pipeline.
    /// </summary>
    /// <param name="action">The action to dispatch.</param>
    /// <remarks>
    /// Dispatch delegates are composed by the store runtime so middleware can intercept or short-circuit action flow.
    /// </remarks>
    public delegate void Dispatch(IAction action);

    /// <summary>
    /// Represents the dispatch capability exposed to middleware at runtime.
    /// </summary>
    /// <remarks>
    /// Middleware receives this context for use by the returned dispatch delegate. Middleware may call
    /// <see cref="Dispatch(IAction)"/> again while a dispatch is already in progress, and those nested actions
    /// participate in the store's total serialized dispatch order. Middleware may also schedule work onto background
    /// execution contexts; any later dispatch from that scheduled work is still serialized by the store, but it is
    /// outside the original dispatch transaction.
    /// </remarks>
    public interface IDispatchContext
    {
        /// <summary>
        /// Dispatches an action to the store.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        void Dispatch(IAction action);

        /// <summary>
        /// Asynchronously dispatches an action to the store. Implementations may perform the dispatch synchronously
        /// and return a completed <see cref="ValueTask"/>, or schedule it to run asynchronously.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        /// <param name="ct">Cancellation token for enqueue-style dispatches.</param>
        ValueTask DispatchAsync(IAction action, CancellationToken ct = default);

        /// <summary>
        /// Enqueues an action to be dispatched from a background context. This method guarantees the caller will
        /// not block waiting for the dispatch to complete.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        /// <param name="ct">Cancellation token for the background enqueue operation.</param>
        Task Enqueue(IAction action, CancellationToken ct = default);
    }

    /// <summary>
    /// Represents middleware that wraps a store dispatch pipeline.
    /// </summary>
    /// <param name="context">The dispatch context available to the middleware at runtime.</param>
    /// <param name="next">The next dispatch delegate in the pipeline.</param>
    /// <returns>A <see cref="Dispatch"/> delegate that may intercept, transform, or forward actions.</returns>
    /// <remarks>
    /// Middleware is composed in registration order. Each delegate decides whether and when to invoke <paramref name="next"/>.
    /// The provided <paramref name="context"/> intentionally exposes only dispatch so middleware can participate in
    /// action flow without taking ownership of store state, subscriptions, or disposal.
    /// </remarks>
    public delegate Dispatch Middleware(IDispatchContext context, Dispatch next);
}