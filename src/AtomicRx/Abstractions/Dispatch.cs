namespace AtomicRx
{
    /// <summary>
    /// Represents a dispatch operation that forwards an <see cref="IAction"/> to a store.
    /// </summary>
    /// <param name="action">The action to dispatch.</param>
    public delegate void Dispatch(IAction action);

    /// <summary>
    /// Represents middleware that wraps a store dispatch pipeline.
    /// </summary>
    /// <param name="store">The store whose dispatch pipeline is being extended.</param>
    /// <param name="next">The next dispatch delegate in the pipeline.</param>
    /// <returns>A <see cref="Dispatch"/> delegate that may intercept, transform, or forward actions.</returns>
    /// <remarks>
    /// Middleware is composed in registration order, with each delegate deciding whether and when to invoke <paramref name="next"/>.
    /// </remarks>
    public delegate Dispatch Middleware(IStore store, Dispatch next);
}