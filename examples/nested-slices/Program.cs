namespace SnapshotStore.Examples.NestedSlices
{
    public interface IFeatureAction : IAction;

    public sealed record SetThemeAction(string Theme) : IFeatureAction;

    public sealed record SetDisplayNameAction(string DisplayName) : IFeatureAction;

    public sealed record UiState(string Theme) : IState;

    public sealed record ProfileState(string DisplayName, string TimeZone) : IState;

    public sealed record UserState(ProfileState Profile) : IState;

    public static class Program
    {
        public static void Main()
        {
            // Path-like names make feature hierarchy recognizable while root state stays flat.
            var uiSlice = StoreFactory.CreateSlice<UiState, IFeatureAction>(
                name: "app/ui",
                initialState: new UiState("light"),
                reducer: static (state, action) => action switch
                {
                    SetThemeAction set => state with { Theme = set.Theme },
                    _ => state
                });

            // Nested state object models deeper domain structure inside a slice.
            var userSlice = StoreFactory.CreateSlice<UserState, IFeatureAction>(
                name: "app/user",
                initialState: new UserState(new ProfileState("Guest", "UTC")),
                reducer: static (state, action) => action switch
                {
                    SetDisplayNameAction set => state with
                    {
                        Profile = state.Profile with { DisplayName = set.DisplayName }
                    },
                    _ => state
                });

            using var store = StoreFactory.CreateStore(uiSlice, userSlice);

            using var subscription = store.Subscribe(static snapshot =>
            {
                var ui = (UiState)snapshot["app/ui"];
                var user = (UserState)snapshot["app/user"];

                Console.WriteLine($"Theme: {ui.Theme} | User: {user.Profile.DisplayName}");
            });

            store.Dispatch(new SetThemeAction("dark"));
            store.Dispatch(new SetDisplayNameAction("FooBar"));
        }
    }
}
