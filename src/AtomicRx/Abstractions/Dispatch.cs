namespace AtomicRx
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
    /// Represents middleware that wraps a store dispatch pipeline.
    /// </summary>
    /// <param name="store">The store whose dispatch pipeline is being extended.</param>
    /// <param name="next">The next dispatch delegate in the pipeline.</param>
    /// <returns>A <see cref="Dispatch"/> delegate that may intercept, transform, or forward actions.</returns>
    /// <remarks>
    /// Middleware is composed in registration order. Each delegate decides whether and when to invoke <paramref name="next"/>.
    /// </remarks>
    public delegate Dispatch AtomicMiddleware(IStore store, Dispatch next);
}