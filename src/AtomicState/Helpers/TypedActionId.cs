namespace SC.AtomicState.Helpers
{
    /// <summary>
    /// Strongly-typed action identifier that carries a static type string.
    /// </summary>
    /// <typeparam name="T">Payload type associated with the action id.</typeparam>
    public readonly record struct TypedActionId<T>(string Type)
    {
        /// <inheritdoc/>
        public override string ToString() => Type;
    }
}
