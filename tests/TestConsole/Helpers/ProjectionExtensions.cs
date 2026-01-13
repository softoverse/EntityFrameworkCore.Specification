using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace TestConsole.Helpers;

/// <summary>
/// Provides extension methods for projecting an IQueryable source to a target type using expression trees.
/// </summary>
public static class ProjectionExtensions
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, LambdaExpression>> ProjectionCache = new();

    /// <summary>
    /// Projects an <see cref="IQueryable{TSource}"/> to an <see cref="IQueryable{TTarget}"/> using dynamically generated expression trees.
    /// </summary>
    /// <typeparam name="TSource">The source entity type.</typeparam>
    /// <typeparam name="TTarget">The target DTO type.</typeparam>
    /// <param name="source">The source queryable to project.</param>
    /// <param name="customMapping">Optional custom mapping expression for complex scenarios.</param>
    /// <returns>An <see cref="IQueryable{TTarget}"/> with the projected properties.</returns>
    public static IQueryable<TTarget> ProjectTo<TSource, TTarget>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, object>>? customMapping = null)
        where TSource : class
    {
        var projection = GetProjection<TSource, TTarget>(customMapping);
        return source.Select(projection);
    }

    /// <summary>
    /// Retrieves or generates a projection expression from the source type to the target type.
    /// </summary>
    /// <typeparam name="TSource">The source entity type.</typeparam>
    /// <typeparam name="TTarget">The target DTO type.</typeparam>
    /// <param name="customMapping">Optional custom mapping expression for complex scenarios.</param>
    /// <returns>An expression representing the projection from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.</returns>
    private static Expression<Func<TSource, TTarget>> GetProjection<TSource, TTarget>(
        Expression<Func<TSource, object>>? customMapping = null)
        where TSource : class
    {
        var sourceType = typeof(TSource);
        var targetType = typeof(TTarget);

        // Only use cache for default projections as custom mappings are highly variable
        if (customMapping == null)
        {
            if (ProjectionCache.TryGetValue(sourceType, out var targetCache) &&
                targetCache.TryGetValue(targetType, out var cachedProjection))
            {
                return (Expression<Func<TSource, TTarget>>)cachedProjection;
            }
        }

        var parameter = Expression.Parameter(sourceType, "entity");
        var bindings = new Dictionary<string, MemberBinding>();
        
        // Use a case-insensitive lookup for matching properties if needed, 
        // but here we follow standard property naming conventions
        var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                         .Where(p => p.CanWrite);

        // Generate default property mappings
        foreach (var targetProperty in targetProperties)
        {
            var sourceProperty = sourceType.GetProperty(targetProperty.Name, BindingFlags.Public | BindingFlags.Instance);

            if (sourceProperty == null || !targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                continue;

            var propertyAccess = Expression.Property(parameter, sourceProperty);
            var binding = Expression.Bind(targetProperty, propertyAccess);
            bindings[targetProperty.Name] = binding;
        }

        // If custom mapping is provided, merge it with default bindings
        if (customMapping != null)
        {
            var visitor = new ParameterUpdateVisitor(customMapping.Parameters[0], parameter);
            var customBody = visitor.Visit(customMapping.Body);

            // Unwrap boxing if any
            if (customBody is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            {
                customBody = unary.Operand;
            }

            // Support anonymous types or new expressions
            if (customBody is NewExpression newExp && newExp.Members != null)
            {
                for (int i = 0; i < newExp.Members.Count; i++)
                {
                    var member = newExp.Members[i];
                    var argument = newExp.Arguments[i];
                    
                    var targetProperty = targetType.GetProperty(member.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (targetProperty != null && targetProperty.CanWrite)
                    {
                        bindings[targetProperty.Name] = Expression.Bind(targetProperty, argument);
                    }
                }
            }
            // Support member initialization: x => new Dest { Prop = x.Value }
            else if (customBody is MemberInitExpression memberInit)
            {
                foreach (var binding in memberInit.Bindings)
                {
                    if (binding is MemberAssignment assignment)
                    {
                        var targetProperty = targetType.GetProperty(binding.Member.Name, BindingFlags.Public | BindingFlags.Instance);
                        if (targetProperty != null && targetProperty.CanWrite)
                        {
                            bindings[targetProperty.Name] = Expression.Bind(targetProperty, assignment.Expression);
                        }
                    }
                }
            }
            else
            {
                // Fallback for single value mapping: match by a property named 'CustomProperty' ONLY if it exists
                var customProperty = targetType.GetProperty("CustomProperty", BindingFlags.Public | BindingFlags.Instance);
                if (customProperty != null && customProperty.CanWrite)
                {
                    bindings[customProperty.Name] = Expression.Bind(customProperty, customBody);
                }
            }
        }

        var constructor = Expression.New(targetType);
        var memberInitFinal = Expression.MemberInit(constructor, bindings.Values);
        var lambda = Expression.Lambda<Func<TSource, TTarget>>(memberInitFinal, parameter);

        // Cache the default projection
        if (customMapping == null)
        {
            var targetCache = ProjectionCache.GetOrAdd(sourceType, _ => new ConcurrentDictionary<Type, LambdaExpression>());
            targetCache[targetType] = lambda;
        }

        return lambda;
    }

    /// <summary>
    /// A visitor to replace the original lambda parameter with the new projection parameter.
    /// </summary>
    private class ParameterUpdateVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterUpdateVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
}