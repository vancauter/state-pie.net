using System.Threading.Tasks;

namespace SCLibs.StateContainer
{
    /// <summary>
    /// Example Thunk middleware factory that mirrors the shape of <see cref="LoggingMiddleware"/>.
    /// This delegates to the builtin implementation in <see cref="BuiltinMiddleware"/>.
    /// </summary>
    public static class ThunkMiddleware
    {
        /// <summary>
        /// Create a thunk middleware instance. This returns a <see cref="Middleware"/> which
        /// will execute function payloads carried inside <see cref="IAction{TPayload}"/> where
        /// the payload is a thunk (see <see cref="BuiltinMiddleware.Thunk"/> for supported shapes).
        /// </summary>
        public static Middleware Create() => BuiltinMiddleware.Thunk();
    }
}
