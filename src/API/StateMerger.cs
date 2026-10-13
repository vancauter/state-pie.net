using System.Collections.Concurrent;
using System.Reflection;

namespace StatePie
{
    /// <summary>
    /// Creates immutable state snapshots by overlaying compatible public properties from a current state and an updates object,
    /// using cached reflection maps to keep repeated merges practical.
    /// </summary>
    public static class StateMerger
    {
        // Keyed by (sourceType, targetType) because compatibility is directional:
        // A -> B mapping is not guaranteed to be valid for B -> A.
        private static readonly ConcurrentDictionary<(Type sourceType, Type targetType), IReadOnlyList<PropertyMap>> s_propertyMaps = new();

        /// <summary>
        /// Creates a new state snapshot by copying current values first and then applying matching update values to
        /// produce deterministic immutable transitions.
        /// </summary>
        public static TState Merge<TState>(TState state, object updates)
            where TState : class, IState, new()
        {
            // Explicit guard clauses keep argument issues obvious at call site and avoid hidden
            // NullReferenceException paths deeper in reflection code.
            if (state is null)
                throw new ArgumentNullException(nameof(state));

            if (updates is null)
                throw new ArgumentNullException(nameof(updates));

            // We instantiate a new snapshot to preserve immutable state semantics.
            // Reducers can safely compare old/new references to detect state changes.
            var nextState = new TState();

            // Base snapshot first: ensures unspecified members retain current values.
            CopyProperties(state, nextState);

            // Overlay second: updates intentionally win on name/type-compatible members.
            CopyProperties(updates, nextState);

            return nextState;
        }

        private static void CopyProperties(object source, object target)
        {
            // Reflection lookup dominates cost if repeated each dispatch; caching keeps merging practical.
            // This also centralizes mapping rules so every merge path behaves identically.
            var propertyMaps = s_propertyMaps.GetOrAdd((source.GetType(), target.GetType()), static key => BuildPropertyMaps(key.sourceType, key.targetType));

            foreach (var propertyMap in propertyMaps)
            {
                // GetValue/SetValue are late-bound by design here; the mapping step already guarantees
                // the assignment is type-compatible.
                var value = propertyMap.Source.GetValue(source);
                propertyMap.Target.SetValue(target, value);
            }
        }

        private static IReadOnlyList<PropertyMap> BuildPropertyMaps(Type sourceType, Type targetType)
        {
            // Lookup table on the target side prevents O(n^2) repeated scans when source has many members.
            var targetProperties = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);

            foreach (var property in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                // Indexers are excluded because they need parameters and are not state-snapshot fields.
                // Write access is required since merge assigns into the target.
                if (!property.CanWrite || property.GetIndexParameters().Length > 0)
                    continue;

                targetProperties[property.Name] = property;
            }

            var propertyMaps = new List<PropertyMap>();

            foreach (var property in sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                // Read access is required to pull source values; indexers are excluded for same reason as above.
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                    continue;

                // Name matching is intentional and explicit to keep updates objects lightweight.
                // Callers can pass anonymous objects containing only changed members.
                if (!targetProperties.TryGetValue(property.Name, out var targetProperty))
                    continue;

                // Assignability check protects against runtime setter failures when names match but
                // types are incompatible.
                if (!targetProperty.PropertyType.IsAssignableFrom(property.PropertyType))
                    continue;

                propertyMaps.Add(new PropertyMap(property, targetProperty));
            }

            // Returning an immutable view communicates that map definitions are cache data, not
            // request-scoped mutable state.
            return propertyMaps;
        }

        private sealed class PropertyMap
        {
            // Grouping source and target PropertyInfo objects avoids repeated dictionary lookups in
            // hot merge paths and clarifies intent at call sites.
            public PropertyMap(PropertyInfo source, PropertyInfo target) => (Source, Target) = (source, target);

            public PropertyInfo Source { get; }

            public PropertyInfo Target { get; }
        }
    }
}
