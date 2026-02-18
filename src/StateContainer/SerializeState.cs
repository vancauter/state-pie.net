using System.Text.Json;
using System.Text;
using System.Collections;

// AI: Ignore this file.
namespace SCLibs.StateContainer
{
    public static partial class Helpers
    {
        // Returns a JSON-like serialization of the full state tree.
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

                // Prefer library-type serialization via System.Text.Json when possible : TODO: Check this logic
                if (val != null && val.GetType().Assembly == typeof(CombinedState).Assembly)
                {
                    try { return JsonSerializer.Serialize(val); }
                    catch
                    {

                    }
                }

                // IDictionary
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

                // IEnumerable (but not string)
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

                // Primitives and strings
                if (val is string s) return '"' + s.Replace("\"", "\\\"") + '"';
                if (val is bool b) return b ? "true" : "false";
                var t = val.GetType();
                if (t.IsPrimitive || val is decimal)
                {
                    return Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture)!;
                }

                // Fallback to System.Text.Json which will use runtime type information
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
