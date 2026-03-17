namespace SC.AtomicState.Runtime
{
    internal sealed partial class StateStream
    {
        /// <summary>
        /// Captures one queued callback so replay, live values, and termination share the same delivery path.
        /// </summary>
        private readonly record struct Notification(NotificationKind Kind, IState? State, Exception? Error)
        {
            public static Notification OnCompleted() => new(NotificationKind.Completed, null, null);

            public static Notification OnError(Exception error) => new(NotificationKind.Error, null, error);

            public static Notification OnNext(IState state) => new(NotificationKind.Next, state, null);
        }

        /// <summary>
        /// Selects which observer callback the queued notification will invoke.
        /// </summary>
        private enum NotificationKind
        {
            Next,
            Error,
            Completed
        }
    }
}