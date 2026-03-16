using System.Text.Json;
using AtomicRx.Runtime;

namespace AtomicRx
{
    /// <summary>
    /// Provides JSON serialization helpers for store state values.
    /// </summary>
    public static class StateJson
    {
        /// <summary>
        /// Serializes an <see cref="IState"/> graph into JSON.
        /// </summary>
        /// <param name="state">The state to serialize.</param>
        /// <returns>A JSON string representation of <paramref name="state"/>.</returns>
        /// <remarks>
        /// When the state is a combined root state created by <see cref="Factory.CombineReducers(ISlice[])"/>, the serializer flattens slice entries into a JSON object keyed by slice name.
        /// </remarks>
        public static string Serialize(IState state)
        {
            static object? Convert(IState currentState)
            {
                if (currentState is CombinedState combinedState)
                {
                    var dictionary = new Dictionary<string, object?>(StringComparer.Ordinal);
                    foreach (var slice in combinedState.States)
                    {
                        dictionary[slice.Key] = Convert(slice.Value);
                    }

                    return dictionary;
                }

                return currentState;
            }

            return JsonSerializer.Serialize(Convert(state));
        }
    }
}