using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace AtomicState
{
    /// <summary>
    /// A delegate that defines the signature of a reducer function.
    /// </summary>
    public delegate IState Reducer(IState state, IAction action);

    /// <summary>
    /// A type-safe delegate that defines the signature of a reducer function for a specific state type.
    /// </summary>
    public delegate TState Reducer<TState>(TState state, IAction action) where TState : IState;

    /// <summary>
    /// A delegate that defines the signature of a root reducer function for a combined store state.
    /// </summary>
    public delegate IReadOnlyDictionary<string, IState> RootReducer(IReadOnlyDictionary<string, IState> state, IAction action);

    /// <summary>
    /// A utility class for merging state objects.
    /// </summary>
    public static class StateMerger
    {
        private static readonly ConcurrentDictionary<(Type sourceType, Type targetType), IReadOnlyList<PropertyMap>> s_propertyMaps = new();

        public static TState Merge<TState>(TState state, object updates)
            where TState : class, IState, new()
        {
            if (state is null)
                throw new ArgumentNullException(nameof(state));

            if (updates is null)
                throw new ArgumentNullException(nameof(updates));

            var nextState = new TState();

            CopyProperties(state, nextState);
            CopyProperties(updates, nextState);

            return nextState;
        }

        private static void CopyProperties(object source, object target)
        {
            var propertyMaps = s_propertyMaps.GetOrAdd((source.GetType(), target.GetType()), static key => BuildPropertyMaps(key.sourceType, key.targetType));

            foreach (var propertyMap in propertyMaps)
            {
                var value = propertyMap.Source.GetValue(source);
                propertyMap.Target.SetValue(target, value);
            }
        }

        private static IReadOnlyList<PropertyMap> BuildPropertyMaps(Type sourceType, Type targetType)
        {
            var targetProperties = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);

            foreach (var property in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanWrite || property.GetIndexParameters().Length > 0)
                    continue;

                targetProperties[property.Name] = property;
            }

            var propertyMaps = new List<PropertyMap>();

            foreach (var property in sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                    continue;

                if (!targetProperties.TryGetValue(property.Name, out var targetProperty))
                    continue;

                if (!targetProperty.PropertyType.IsAssignableFrom(property.PropertyType))
                    continue;

                propertyMaps.Add(new PropertyMap(property, targetProperty));
            }

            return propertyMaps;
        }

        private sealed class PropertyMap
        {
            public PropertyMap(PropertyInfo source, PropertyInfo target) => (Source, Target) = (source, target);

            public PropertyInfo Source { get; }

            public PropertyInfo Target { get; }
        }
    }

}