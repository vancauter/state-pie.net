namespace AtomicState
{
    internal sealed class Slice<T> : ISlice where T : IState
    {
        public Slice(string name, T initialState, Reducer<T> reducer)
        {
            Name = name;
            InitialState = initialState;
            Reducer = (state, action) => reducer((T)state, action);
        }

        public string Name { get; }

        public IState InitialState { get; }

        public Reducer Reducer { get; }
    }
}