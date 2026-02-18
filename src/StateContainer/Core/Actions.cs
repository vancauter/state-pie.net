namespace SCLibs.StateContainer.Core
{
    // Intentionally named `Action` to represent domain actions within this library.
    // Note: this can shadow `System.Action` in some scopes; be explicit if that causes ambiguity.
    internal sealed record Action(string Type) : IAction;

    internal sealed record Action<TPayload>(string Type, TPayload Payload) : IAction<TPayload>;
}