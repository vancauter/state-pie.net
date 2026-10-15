using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace StatePie
{
    /// <summary>
    /// Creates immutable state snapshots efficiently using compiled expression delegates and shallow cloning.
    /// </summary>
    public static class StateMerger
    {
        private static readonly ConcurrentDictionary<(Type sourceType, Type targetType), IReadOnlyList<Action<object, object>>> s_copierCache = new();
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

            // 1. Fast shallow clone of the existing state
            var cloneFunc = s_cloneCache.GetOrAdd(typeof(TState), static type =>
            {
                var param = Expression.Parameter(typeof(object), "obj");
                var cast = Expression.Convert(param, type);
                var method = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!;
                var call = Expression.Call(cast, method);
                return Expression.Lambda<Func<object, object>>(Expression.Convert(call, typeof(object)), param).Compile();
            });

            var nextState = (TState)cloneFunc(state);

            // 2. Apply only the update properties using compiled delegates
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

                var sourceParam = Expression.Parameter(typeof(object), "source");
                var targetParam = Expression.Parameter(typeof(object), "target");

                var castSource = Expression.Convert(sourceParam, sourceType);
                var castTarget = Expression.Convert(targetParam, targetType);

                var getExpr = Expression.Property(castSource, property);
                var setExpr = Expression.Call(castTarget, targetProperty.GetSetMethod()!, getExpr);

                var copier = Expression.Lambda<Action<object, object>>(setExpr, sourceParam, targetParam).Compile();
                copiers.Add(copier);
            }

            return copiers;
        }
    }
}

