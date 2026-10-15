using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace StatePie
{
    /// <summary>
    /// Creates immutable state snapshots efficiently using compiled expression delegates and shallow cloning.
    /// Tradeoff: Offers high developer ergonomics and dynamic partial property merging via anonymous objects,
    /// but incurs a small runtime overhead from expression compilation and late-bound property mapping
    /// compared to direct manual record 'with' expressions or compile-time initializers.
    /// </summary>
    public static class StateMerger
    {
        // Keyed by (sourceType, targetType) because compatibility is directional (A -> B != B -> A).
        private static readonly ConcurrentDictionary<(Type sourceType, Type targetType), IReadOnlyList<Action<object, object>>> s_copierCache = new();

        // Cached per target state type to avoid regenerating the MemberwiseClone lambda expression.
        private static readonly ConcurrentDictionary<Type, Func<object, object>> s_cloneCache = new();

        /// <summary>
        /// Creates a new state snapshot by cloning the current state and overlaying matching update values.
        /// </summary>
        public static TState Merge<TState>(TState state, object updates) where TState : class, IState, new()
        {
            if (state is null)
                throw new ArgumentNullException(nameof(state));

            if (updates is null)
                throw new ArgumentNullException(nameof(updates));

            // 1. Fast shallow clone of existing state (avoids full reflection copy of all unmodified fields).
            var cloneFunc = s_cloneCache.GetOrAdd(typeof(TState), static type =>
            {
                var param = Expression.Parameter(typeof(object), "obj");
                var cast = Expression.Convert(param, type);

                // MemberwiseClone is protected on object; reflection gives us direct low-level instance duplication.
                var method = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
                var call = Expression.Call(cast, method);
                return Expression.Lambda<Func<object, object>>(Expression.Convert(call, typeof(object)), param).Compile();
            });

            var nextState = (TState)cloneFunc(state);

            // 2. Apply only the update properties using compiled delegates (cached for thread-safe hot-path reuse).
            var copiers = s_copierCache.GetOrAdd(
                (updates.GetType(), typeof(TState)),
                static key => BuildCopiers(key.sourceType, key.targetType));

            foreach (var copier in copiers)
            {
                copier(updates, nextState);
            }

            return nextState;
        }

        private static IReadOnlyList<Action<object, object>> BuildCopiers(Type sourceType, Type targetType)
        {
            // Indexing target properties prevents O(n^2) nested scans when source types contain many members.
            var targetProperties = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
            foreach (var property in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanWrite || property.GetIndexParameters().Length > 0 || property.GetSetMethod() == null)
                    continue;

                targetProperties[property.Name] = property;
            }

            var copiers = new List<Action<object, object>>();

            foreach (var property in sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0 || property.GetGetMethod() == null)
                    continue;

                if (!targetProperties.TryGetValue(property.Name, out var targetProperty))
                    continue;

                if (!targetProperty.PropertyType.IsAssignableFrom(property.PropertyType))
                    continue;

                // Compile dynamic getter/setter actions to bypass slow reflection GetValue/SetValue calls during execution.
                var sourceParam = Expression.Parameter(typeof(object), "source");
                var targetParam = Expression.Parameter(typeof(object), "target");

                var castSource = Expression.Convert(sourceParam, sourceType);
                var castTarget = Expression.Convert(targetParam, targetType);

                var getExpr = Expression.Property(castSource, property);
                var setExpr = Expression.Call(castTarget, targetProperty.GetSetMethod(), getExpr);

                var copier = Expression.Lambda<Action<object, object>>(setExpr, sourceParam, targetParam).Compile();
                copiers.Add(copier);
            }

            return copiers;
        }
    }
}
