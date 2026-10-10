namespace SnapshotStore
{
    internal sealed class Slice<T> : ISlice where T : IState
    {
        public Slice(string sliceName, T initialState, Reducer<T> reducer)
        {
            Name = sliceName;
            InitialState = initialState;
            Reducer = (state, action) => reducer((T)state, action);
        }

        public string Name { get; }

        public IState InitialState { get; }

        public Reducer Reducer { get; }
    }
}