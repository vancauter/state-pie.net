namespace SnapshotStore
{
    /// <summary>
    /// Internal implementation of IAction.
    /// </summary>
    internal readonly struct Action : IAction
    {
        public string Type { get; }

        // Constructor is internal to prevent external instantiation.
        internal Action(string type) => Type = type;
    }

    /// <summary>
    /// Internal implementation of IAction<TPayload>.
    /// </summary>
    internal readonly struct Action<TPayload> : IAction<TPayload> where TPayload : notnull
    {
        public string Type { get; }

        public TPayload Payload { get; }

        // Constructor is internal to prevent external instantiation.
        internal Action(string type, TPayload payload) => (Type, Payload) = (type, payload);
    }
}