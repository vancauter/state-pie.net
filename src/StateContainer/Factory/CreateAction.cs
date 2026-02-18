namespace SCLibs.StateContainer
{
    public static partial class Factory
    {
        public static IAction CreateAction(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));

            return new Action(type);
        }

        public static IAction CreateAction<TPayload>(string type, TPayload payload)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));

            return new Action<TPayload>(type, payload);
        }
    }
}
