using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.EntityFrameworkCore.Query;

namespace Softoverse.EntityFrameworkCore.Specification.Helpers;

public static class ExpressionGenerator<TEntity>
{
    private static readonly ConcurrentDictionary<Expression, Func<TEntity, object>> PropertyCompiledSelectors = new ConcurrentDictionary<Expression, Func<TEntity, object>>();
    private static readonly ConcurrentDictionary<string, LambdaExpression> PropertySelectorCache = new ConcurrentDictionary<string, LambdaExpression>();

    static readonly MethodInfo SetPropertyMethodInfo;

    static ExpressionGenerator()
    {
        SetPropertyMethodInfo = typeof(SetPropertyCalls<TEntity>)
                                .GetMethods()
                                .First(m => m.Name == nameof(SetPropertyCalls<TEntity>.SetProperty) && !m.GetParameters()[1].ParameterType.IsGenericType);
    }

    public static Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> BuildUpdateExpression(ICollection<Expression<Func<TEntity, object>>> properties,
                                                                                                               TEntity model)
    {
        var parameter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>), "c");
        Expression body = parameter;

        foreach (var propertySelector in properties)
        {
            var compiledSelector = PropertyCompiledSelectors.GetOrAdd(propertySelector.Body, _ => propertySelector.Compile());

            object value = compiledSelector(model);
            Type propertyType = value.GetType();

            var method = SetPropertyMethodInfo.MakeGenericMethod(propertyType);

            var propertyExpression = propertySelector.Body.Type == propertyType
                ? propertySelector.Body
                : Expression.Convert(propertySelector.Body, propertyType);

            var convertedPropertySelector = Expression.Lambda(propertyExpression, propertySelector.Parameters);

            body = Expression.Call(body, method, convertedPropertySelector, Expression.Constant(value, propertyType));
        }

        return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(body, parameter);
    }

    public static Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> BuildUpdateExpression(IDictionary<string, object> propertyUpdates)
    {
        var parameter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>), "c");
        Expression body = parameter;

        foreach (var update in propertyUpdates)
        {
            var propertyPath = update.Key;
            var value = update.Value;

            // Retrieve or create cached property selector
            var propertySelector = PropertySelectorCache.GetOrAdd(propertyPath, key =>
            {
                var entityParameter = Expression.Parameter(typeof(TEntity), "e");
                Expression propertyAccess = entityParameter;

                foreach (var property in key.Split('.'))
                {
                    propertyAccess = Expression.PropertyOrField(propertyAccess, property);
                }

                return Expression.Lambda(propertyAccess, entityParameter);
            });

            Type propertyType = propertySelector.Body.Type;

            // Handle Nullable Types
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = Nullable.GetUnderlyingType(propertyType);
            }

            object convertedValue;
            try
            {
                // Ensure value is convertible
                convertedValue = value == null ? null : Convert.ChangeType(value, propertyType);
            }
            catch (InvalidCastException)
            {
                // If Convert.ChangeType fails, try direct assignment if compatible
                convertedValue = value;
            }

            var method = SetPropertyMethodInfo.MakeGenericMethod(propertyType);

            body = Expression.Call(body, method, propertySelector, Expression.Constant(convertedValue, propertyType));
        }

        return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(body, parameter);
    }
}