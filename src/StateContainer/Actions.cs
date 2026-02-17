namespace StateContainer
{
    internal sealed record StoreAction(string Type) : IAction;
    internal sealed record StoreAction<TPayload>(string Type, TPayload Payload) : IAction<TPayload>;
}
