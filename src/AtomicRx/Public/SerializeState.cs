using System.Text.Json;
using AtomicRx.Core;

namespace AtomicRx
{
    public static class Helpers
    {
        /// <summary>
        /// This method produces a human-readable representation of <see cref="IState"/>,
        /// including <see cref="CombinedState"/> instances and common collection types.
        /// </summary>
        /// <param name="state">The state to serialize.</param>
        /// <returns>A JSON string representation.</returns>
        public static string SerializeState(IState state)
        {
            // Recursively convert the internal state tree into CLR objects that JsonSerializer
            // can cleanly convert to JSON. The key idea: CombinedState represents a map of
            // named slice states (the dictionary `States`), whereas leaf states are concrete
            // record/class instances implementing `IState`.
            static object? convert(IState s)
            {
                // If this is a CombinedState, produce a Dictionary<string, object?> where
                // each key is the slice name and the value is the converted child state.
                if (s is CombinedState combined)
                {
                    var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
                    foreach (var kv in combined.States)
                    {
                        // Recurse for nested CombinedState instances as well.
                        dict[kv.Key] = convert(kv.Value);
                    }
                    return dict;
                }

                // For non-combined (leaf) states, return the instance directly and allow
                // `JsonSerializer` to reflect over its public properties. This keeps the
                // serializer simple and compatible with record types and POCOs used as state.
                return s;
            }

            // Build the CLR representation and hand it to JsonSerializer. Consumers who
            // want different formatting (indented, naming policy, etc.) can replace this
            // call or we can expose options later; keep default behavior here for tests.
            var built = convert(state);
            return JsonSerializer.Serialize(built);
        }
    }
}
