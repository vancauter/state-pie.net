namespace SC.AtomicState.Runtime
{
    internal sealed partial class StateStream
    {
        /// <summary>
        /// Keeps unsubscribe tied to the specific registration created for the observer.
        /// </summary>
        private sealed class Subscription(StateStream owner, Registration registration) : IDisposable
        {
            /// <summary>
            /// Carries the stream instance that owns the registration.
            /// </summary>
            private readonly StateStream _owner = owner;

            /// <summary>
            /// Carries the registration to remove when the subscription is disposed.
            /// </summary>
            private readonly Registration _registration = registration;

            /// <summary>
            /// Stops future live delivery for this registration.
            /// </summary>
            public void Dispose() => _owner.Unsubscribe(_registration);
        }
    }
}