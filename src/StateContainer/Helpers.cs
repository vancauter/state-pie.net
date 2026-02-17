using System.Text.Json;
using System.Text;
using System.Collections;

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
                if (val is IEnumerable ie && val is not string)
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

        // Actions
        public static IAction CreateAction(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));

            return new StoreAction(type);
        }

        public static IAction CreateActionPayload<TPayload>(string type, TPayload payload)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));

            return new StoreAction<TPayload>(type, payload);
        }

        // CreateSlice
        public static Slice<TState> CreateSlice<TState>(string name, TState initialState, Func<TState, IAction, TState> reducerFn) where TState : IState
            => new(name, initialState, reducerFn);

        // CombineReducers
        public static (CombinedState initialState, Reducer rootReducer) CombineReducers(params ISlice[] slices)
        {
            if (slices.Length == 0)
                throw new ArgumentException("Must provide at least one slice to combine.", nameof(slices));

            // detect duplicate slice names early for clearer error messages
            var duplicateGroup = slices.GroupBy(s => s.Name).FirstOrDefault(g => g.Count() > 1);
            if (duplicateGroup != null)
                throw new ArgumentException($"Duplicate slice name detected: {duplicateGroup.Key}", nameof(slices));

            var reducerDictionary = slices.ToDictionary(slice => slice.Name, slice => slice.Reducer);
            var initialSliceStates = slices.ToDictionary(slice => slice.Name, slice => slice.State);
            var initialRootState = CombinedState.Create(initialSliceStates);

            IState rootReducerDelegate(IState state, IAction action)
            {
                if (state is not CombinedState currentRootState)
                {
                    throw new InvalidOperationException(
                        $"[CombineReducers] Unexpected state type: {state?.GetType().Name}. Expected {nameof(CombinedState)}."
                    );
                }
                var nextSliceStates = new Dictionary<string, IState>(currentRootState.States);
                var changed = false;

                foreach (var entity in reducerDictionary)
                {
                    var sliceName = entity.Key;
                    var sliceReducer = entity.Value;

                    if (!currentRootState.States.TryGetValue(sliceName, out var currentSliceState))
                    {
                        throw new InvalidOperationException(
                            $"[CombineReducers] Missing slice state for key: {sliceName} in current root state."
                        );
                    }

                    var nextSliceState = sliceReducer(currentSliceState, action);

                    // use reference-equality to detect changes consistently with the store
                    if (!ReferenceEquals(nextSliceState, currentSliceState))
                    {
                        nextSliceStates[sliceName] = nextSliceState;
                        changed = true;
                    }
                }

                return changed ? CombinedState.Create(nextSliceStates) : currentRootState;
            }

            return (initialRootState, rootReducerDelegate);
        }
    }
}
