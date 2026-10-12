namespace StatePie
{
    /// <summary>
    /// Intercepts a typed action before forwarding it to the next dispatch stage; middleware executes synchronously in registration order, may stop dispatch by not calling next, must call next at most once, and exceptions propagate to the caller.
    /// </summary>
    /// <typeparam name="TAction">The action family accepted by the store.</typeparam>
    /// <param name="action">The action currently moving through the dispatch pipeline.</param>
    /// <param name="next">Forwards the action to the next middleware or reducer pipeline.</param>
    public delegate void DispatchMiddleware<TAction>(TAction action, Action<TAction> next)
        where TAction : class, IAction;
}