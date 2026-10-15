namespace StatePie.Examples.Effects
{
    public interface IProfileAction : IAction;
    public sealed record LoadProfileAction(string Id) : IProfileAction;
    public sealed record ProfileLoadedAction(string Name, string Surname) : IProfileAction;
    public sealed record ProfileFailedAction(string Reason) : IProfileAction;
    public sealed record ProfileState(string Status, string? Name, string? Surname) : IState;

    public static class Program
    {
        public static async Task Main()
        {
            var profileSlice = StoreFactory.CreateSlice<ProfileState, IProfileAction>(
                name: "profile",
                initialState: new ProfileState("idle", null, null),
                reducer: static (state, action) => action switch
                {
                    LoadProfileAction => state with { Status = "loading" },
                    ProfileLoadedAction loaded => state with { Status = "loaded", Name = loaded.Name, Surname = loaded.Surname },
                    ProfileFailedAction failed => state with { Status = $"failed ({failed.Reason})" },
                    _ => state
                });

            // Reducers stay pure: the effect starts after LoadProfileAction commits and reports back with a typed action.
            static async Task loadProfile(IProfileAction action, IEffectContext<IProfileAction> context)
            {
                if (action is not LoadProfileAction load)
                    return;

                var (name, surname) = await FetchNameAsync(load.Id, context.CancellationToken);
                context.Dispatch(name is null || surname is null
                    ? new ProfileFailedAction("not found")
                    : new ProfileLoadedAction(name, surname));
            }

            using var store = StoreFactory.CreateStore(
                [profileSlice],
                [], // optional middleware
                [loadProfile],
                static (exception, action) => Console.WriteLine($"Unexpected effect failure for {action}: {exception.Message}"));

            foreach (var id in new[] { "jane", "missing" })
            {
                var settled = new TaskCompletionSource();
                using var subscription = store.Subscribe(snapshot =>
                {
                    var profile = (ProfileState)snapshot["profile"];
                    Console.WriteLine($"Status: {profile.Status}");

                    if (profile.Status != "loading")
                        _ = settled.TrySetResult();
                });

                store.Dispatch(new LoadProfileAction(id));

                // Dispatch does not wait for effects, so the example waits for the result action to be reduced.
                await settled.Task;
            }
        }

        // Stands in for real I/O; the token stops the delay when the store is disposed.
        private static async Task<(string? name, string? surname)> FetchNameAsync(string id, CancellationToken cancellationToken)
        {
            await Task.Delay(50, cancellationToken);
            return id == "jane" ? ("Jane", "Doe") : (null, null);
        }
    }
}
