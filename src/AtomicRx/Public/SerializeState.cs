using System.Text.Json;
using AtomicRx.Core;

namespace AtomicRx
{
    /// <summary>
    /// Provides helper methods for inspecting state values.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Serializes an <see cref="IState"/> graph into JSON.
        /// </summary>
        /// <param name="state">The state to serialize.</param>
        /// <returns>A JSON string representation of <paramref name="state"/>.</returns>
        /// <remarks>
        /// <see cref="CombinedState"/> instances are flattened into nested dictionaries before serialization so each slice name becomes a JSON property.
        /// </remarks>
        public static string SerializeState(IState state)
        {
            static object? convert(IState s)
            {
                if (s is CombinedState combined)
                {
                    var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
                    foreach (var kv in combined.States)
                    {
                        dict[kv.Key] = convert(kv.Value);
                    }

                    return dict;
                }

                return s;
            }

            var built = convert(state);
            return JsonSerializer.Serialize(built);
        }
    }
}
