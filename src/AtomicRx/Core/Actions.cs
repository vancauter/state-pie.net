namespace SCLibs.StateContainer.Core
{
    internal sealed record Action(string Type) : IAction;
    internal sealed record Action<TPayload>(string Type, TPayload Payload) : IAction<TPayload>;
}