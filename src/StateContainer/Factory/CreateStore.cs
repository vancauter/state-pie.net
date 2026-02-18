using SCLibs.StateContainer.Core;

namespace SCLibs.StateContainer
{
    public static partial class Factory
    {
        public static IStore CreateStore(IState initialState, ReducerFn rootReducer)
            => new Store(initialState, rootReducer);
    }
}