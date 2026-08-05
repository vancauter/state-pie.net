namespace AtomicState
{
    public interface ISlice
    {
        string Name { get; }

        IState InitialState { get; }

        Reducer Reducer { get; }
    }
}