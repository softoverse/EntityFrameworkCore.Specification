using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

using Microsoft.EntityFrameworkCore.Query;

namespace Softoverse.EntityFrameworkCore.Specification.Helpers;

public static class ExpressionGenerator<TEntity>
{
    private static readonly ConcurrentDictionary<Expression, Func<TEntity, object>> PropertyCompiledSelectors = new ConcurrentDictionary<Expression, Func<TEntity, object>>();


    // ReSharper disable once StaticMemberInGenericType
    static readonly MethodInfo SetPropertyMethodInfo;
    static ExpressionGenerator()
    {
        SetPropertyMethodInfo = typeof(UpdateSettersBuilder<TEntity>)
                                .GetMethods()
                                .First(m => m.Name == nameof(UpdateSettersBuilder<TEntity>.SetProperty) && !m.GetParameters()[1].ParameterType.IsGenericType);
    }

    public static Action<UpdateSettersBuilder<TEntity>> BuildUpdateExpression(ICollection<Expression<Func<TEntity, object>>> properties,
                                                                                                                       TEntity model)
    {
        var parameter = Expression.Parameter(typeof(UpdateSettersBuilder<TEntity>), "c");
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

        body = Expression.Block(typeof(void), body);
        return Expression.Lambda<Action<UpdateSettersBuilder<TEntity>>>(body, parameter).Compile();
    }

    public static Action<UpdateSettersBuilder<TEntity>> BuildUpdateExpression(IDictionary<string, object> propertyUpdates)
    {
        var parameter = Expression.Parameter(typeof(UpdateSettersBuilder<TEntity>), "c");
        Expression body = parameter;
        var entityParameter = Expression.Parameter(typeof(TEntity), "e");
        var properties = new Dictionary<Expression<Func<TEntity, object>>, object>();

        foreach (var kvp in propertyUpdates)
        {
            string propertyPath = kvp.Key;
            Expression propertyExpression = entityParameter;

            foreach (var propertyName in propertyPath.Split('.'))
            {
                propertyExpression = Expression.Property(propertyExpression, propertyName);
            }

            var lambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(propertyExpression, typeof(object)), entityParameter);
            object value = kvp.Value;
            if (value is JsonElement jsonElement)
            {
                value = jsonElement.ConvertJsonElement();
            }
            properties[lambda] = value;
        }

        foreach (var kvp in properties)
        {
            var propertySelector = kvp.Key;
            var value = kvp.Value;
            Type propertyType = value.GetType();

            var method = SetPropertyMethodInfo.MakeGenericMethod(propertyType);

            Expression propertyExpression = propertySelector.Body;
            if (propertyExpression.Type != propertyType)
            {
                propertyExpression = Expression.Convert(propertyExpression, propertyType);
            }

            var convertedPropertySelector = Expression.Lambda(propertyExpression, propertySelector.Parameters);

            Expression valueExpression = Expression.Constant(value, propertyType);
            if (propertyType.IsValueType)
            {
                valueExpression = Expression.Convert(valueExpression, propertyType);
            }

            body = Expression.Call(body, method, convertedPropertySelector, valueExpression);
        }

        body = Expression.Block(typeof(void), body);
        return Expression.Lambda<Action<UpdateSettersBuilder<TEntity>>>(body, parameter).Compile();
    }
}