namespace SCLibs.StateContainer
{
    /// <summary>
    /// Delegate used to dispatch an <see cref="IAction"/> to the store.
    /// </summary>
    /// <param name="action">Action to dispatch.</param>
    public delegate void Dispatch(IAction action);

    /// <summary>
    /// Middleware factory delegate. Given the store and the next dispatch function,
    /// returns a dispatch function that can intercept or augment actions.
    /// </summary>
    /// <param name="store">The target store.</param>
    /// <param name="next">The next dispatch function in the pipeline.</param>
    /// <returns>A dispatch function that handles the action (and typically calls <paramref name="next"/>).</returns>
    public delegate Dispatch Middleware(IStore store, Dispatch next);
}
