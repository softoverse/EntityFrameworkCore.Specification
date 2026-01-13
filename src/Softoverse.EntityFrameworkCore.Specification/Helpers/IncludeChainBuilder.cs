using System.Linq.Expressions;

namespace Softoverse.EntityFrameworkCore.Specification.Helpers;

/// <summary>
/// Helper class to build navigation property paths for Include operations
/// </summary>
internal static class IncludePathBuilder
{
    /// <summary>
    /// Extracts the property path from a lambda expression
    /// </summary>
    public static string GetPropertyPath<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> expression)
    {
        return GetPropertyPathInternal(expression.Body);
    }
    
    /// <summary>
    /// Extracts the property path from a lambda expression for ThenInclude scenarios
    /// </summary>
    public static string GetThenIncludePropertyPath<TPrevious, TProperty>(Expression<Func<TPrevious, TProperty>> expression)
    {
        return GetPropertyPathInternal(expression.Body);
    }

    private static string GetPropertyPathInternal(Expression expression)
    {
        // Remove any Convert/ConvertChecked wrapping
        while (expression is UnaryExpression unary && 
               (expression.NodeType == ExpressionType.Convert || 
                expression.NodeType == ExpressionType.ConvertChecked))
        {
            expression = unary.Operand;
        }

        // Check if this is a method call (like .Where()) - filtered includes
        if (expression is MethodCallExpression)
        {
            // For any method call (Where, OrderBy, etc.), we return empty to signal
            // that this is a complex/filtered include that should use IncludeActions
            return string.Empty;
        }

        if (expression is MemberExpression memberExpression)
        {
            var parentPath = string.Empty;
            
            // Check if there's a parent member access
            if (memberExpression.Expression is MemberExpression parentMember)
            {
                parentPath = GetPropertyPathInternal(parentMember) + ".";
            }
            
            return parentPath + memberExpression.Member.Name;
        }

        if (expression is ParameterExpression)
        {
            return string.Empty;
        }

        // For complex expressions, return empty - signals use of IncludeActions
        return string.Empty;
    }
}

