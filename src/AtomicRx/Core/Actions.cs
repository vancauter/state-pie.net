namespace SCLibs.StateContainer.Core
{
    // Internal concrete action implementations used by Factory.CreateAction.
    // These are simple record types that implement the public IAction / IAction<TPayload> interfaces.
    internal sealed record Action(string Type) : IAction;

    internal sealed record Action<TPayload>(string Type, TPayload Payload) : IAction<TPayload>;
}