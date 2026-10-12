namespace StatePie
{
    /// <summary>
    /// Represents one named state partition with a typed reducer, ensuring all slice behavior is bound to a single action family.
    /// </summary>
    public interface ISlice<TAction> where TAction : class, IAction
    {
        string Name { get; }

        IState InitialState { get; }

        Reducer<TAction> Reducer { get; }
    }
}