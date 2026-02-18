using System.Collections;
using System.Text;
using System.Text.Json;
using AtomicRx.Core;

namespace AtomicRx
{
    public static partial class Helpers
    {
        /// <summary>
        /// Helper utilities related to state (serialization, debugging, etc.).
        /// </summary>
        public static partial class HelpersInternal { }

        /// <summary>
        /// Returns a JSON-like serialization of the provided state tree.
        /// <para>This method attempts to produce a human-readable representation of <see cref="IState"/>,
        /// including <see cref="CombinedState"/> instances and common collection types.</para>
        /// </summary>
        /// <param name="state">The root state to serialize.</param>
        /// <returns>A JSON-like string representation. Best-effort; not guaranteed to be round-trip serializable.</returns>
        public static string SerializeState(IState state)
        {
            return serializeValue(state);

            static string serializeValue(object? val)
            {
                if (val is null) return "null";

                // CombinedState: serialize its dictionary of slice states
                if (val is CombinedState combined)
                {
                    var sb = new StringBuilder();
                    _ = sb.Append('{');
                    var first = true;
                    foreach (var kv in combined.States)
                    {
                        if (!first) _ = sb.Append(", ");
                        _ = sb.Append('"').Append(kv.Key).Append('"').Append(": ");
                        _ = sb.Append(serializeValue(kv.Value));
                        first = false;
                    }
                    _ = sb.Append('}');
                    return sb.ToString();
                }

                // Prefer library-type serialization via System.Text.Json when possible.
                // We try to use System.Text.Json for types from this assembly for a reasonable default.
                if (val != null && val.GetType().Assembly == typeof(CombinedState).Assembly)
                {
                    try { return JsonSerializer.Serialize(val); }
                    catch
                    {

                    }
                }

                // IDictionary: emit as JSON object with string keys
                if (val is IDictionary dict)
                {
                    var sb = new StringBuilder();
                    _ = sb.Append('{');
                    var first = true;
                    foreach (DictionaryEntry entry in dict)
                    {
                        if (!first) _ = sb.Append(", ");
                        _ = sb.Append('"').Append(entry.Key?.ToString()).Append('"').Append(": ");
                        _ = sb.Append(serializeValue(entry.Value));
                        first = false;
                    }
                    _ = sb.Append('}');
                    return sb.ToString();
                }

                // IEnumerable (but not string): emit as JSON array
                if (val is IEnumerable ie and not string)
                {
                    var sb = new StringBuilder();
                    _ = sb.Append('[');
                    var first = true;
                    foreach (var item in ie)
                    {
                        if (!first) _ = sb.Append(", ");
                        _ = sb.Append(serializeValue(item));
                        first = false;
                    }
                    _ = sb.Append(']');
                    return sb.ToString();
                }

                // Primitives and strings: produce JSON-friendly literals
                if (val is string s) return '"' + s.Replace("\"", "\\\"") + '"';
                if (val is bool b) return b ? "true" : "false";
                if (val is null) return "null";

                var t = val.GetType();
                if (t.IsPrimitive || val is decimal)
                {
                    return Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture)!;
                }

                // Fallback to System.Text.Json which will use runtime type information, or fall back to ToString().
                try
                {
                    return JsonSerializer.Serialize(val);
                }
                catch
                {
                    return '"' + val.ToString()?.Replace("\"", "\\\"") + '"';
                }
            }
        }
    }
}