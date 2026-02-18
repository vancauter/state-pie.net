using SCLibs.StateContainer.Core;

namespace SCLibs.StateContainer
{
    public static partial class Factory
    {
        public static ISlice CreateSlice<TState>(string name, TState initialState, ReducerFn<TState> reducer) where TState : IState
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Slice name must be a non-empty string.", nameof(name));

            return new Slice<TState>(name, initialState, reducer);
        }
    }
}