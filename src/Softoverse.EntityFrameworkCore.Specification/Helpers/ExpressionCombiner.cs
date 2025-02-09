using System.Linq.Expressions;

namespace Softoverse.EntityFrameworkCore.Specification.Helpers;

internal static class ExpressionCombiner
{
    internal static Expression<Func<TEntity, bool>> And<TEntity>(Expression<Func<TEntity, bool>> expression1, Expression<Func<TEntity, bool>> expression2)
    {
        if (expression1 == True<TEntity>()) return expression2;
        if (expression2 == True<TEntity>()) return expression1;

        var parameter = Expression.Parameter(typeof(TEntity));

        var left = ParameterRebinder.ReplaceParameters(parameter, expression1.Body);
        var right = ParameterRebinder.ReplaceParameters(parameter, expression2.Body);

        var body = Expression.AndAlso(left, right);
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        //// Less Efficient
        //var parameter = Expression.Parameter(typeof(TEntity));
        //var body = Expression.AndAlso(Expression.Invoke(expression1, parameter), Expression.Invoke(expression2, parameter));
        //return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    internal static Expression<Func<TEntity, bool>> Or<TEntity>(Expression<Func<TEntity, bool>> expression1, Expression<Func<TEntity, bool>> expression2)
    {
        if (expression1 == True<TEntity>()) return expression2;
        if (expression2 == True<TEntity>()) return expression1;

        var parameter = Expression.Parameter(typeof(TEntity));

        var left = ParameterRebinder.ReplaceParameters(parameter, expression1.Body);
        var right = ParameterRebinder.ReplaceParameters(parameter, expression2.Body);

        var body = Expression.OrElse(left, right);
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        //// Less Efficient
        //var parameter = Expression.Parameter(typeof(TEntity));
        //var body = Expression.OrElse(Expression.Invoke(expression1, parameter), Expression.Invoke(expression2, parameter));
        //return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    internal static Expression<Func<TEntity, bool>> Not<TEntity>(Expression<Func<TEntity, bool>> expression)
    {
        if (expression == True<TEntity>()) return False<TEntity>();
        if (expression == False<TEntity>()) return True<TEntity>();

        var parameter = Expression.Parameter(typeof(TEntity));

        var body = Expression.Not(ParameterRebinder.ReplaceParameters(parameter, expression.Body));
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        //// Less Efficient
        //var parameter = Expression.Parameter(typeof(TEntity));
        //var body = Expression.Not(Expression.Invoke(expression, parameter));
        //return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    public static Expression<Func<TEntity, bool>> CombineWithAnd<TEntity>(IEnumerable<Expression<Func<TEntity, bool>>> expressions)
    {
        Expression<Func<TEntity, bool>> combined = null;

        foreach (var expression in expressions)
        {
            combined = combined == null ? expression : And(combined, expression);
        }

        if (combined == null)
            return True<TEntity>();

        return combined;
    }

    public static Expression<Func<TEntity, bool>> CombineWithOr<TEntity>(IEnumerable<Expression<Func<TEntity, bool>>> expressions)
    {
        Expression<Func<TEntity, bool>> combined = null;

        foreach (var expression in expressions)
        {
            combined = combined == null ? expression : Or(combined, expression);
        }

        if (combined == null)
            return True<TEntity>();

        return combined;
    }

    public static Expression<Func<TEntity, bool>> CombineWithNot<TEntity>(IEnumerable<Expression<Func<TEntity, bool>>> expressions)
    {
        Expression<Func<TEntity, bool>> combined = null;

        foreach (var expression in expressions)
        {
            combined = combined == null ? expression : And(combined, Not(expression));
        }

        if (combined == null)
            return True<TEntity>();

        return combined;
    }

    public static Expression<Func<TEntity, bool>> True<TEntity>()
    {
        return x => true;
    }

    public static Expression<Func<TEntity, bool>> False<TEntity>()
    {
        return x => false;
    }

    public static Expression<Func<TEntity, bool>> When<TEntity>(bool condition, Expression<Func<TEntity, bool>> expression)
    {
        return condition ? expression : True<TEntity>();
    }


    // With Predicates
    public static Expression<Func<TEntity, bool>> ToExpression<TEntity>(Func<TEntity, bool> predicate)
    {
        if (predicate == null) return True<TEntity>();
        return x => predicate(x);
    }

    public static Expression<Func<TEntity, bool>> And<TEntity>(Func<TEntity, bool> predicate1, Func<TEntity, bool> predicate2)
    {
        return x => predicate1(x) && predicate2(x);
    }

    public static Expression<Func<TEntity, bool>> Or<TEntity>(Func<TEntity, bool> predicate1, Func<TEntity, bool> predicate2)
    {
        return x => predicate1(x) || predicate2(x);
    }

    public static Expression<Func<TEntity, bool>> Not<TEntity>(Func<TEntity, bool> predicate)
    {
        return x => !predicate(x);
    }

    public static Expression<Func<TEntity, bool>> When<TEntity>(bool condition, Func<TEntity, bool> predicate)
    {
        return condition ? ToExpression(predicate) : True<TEntity>();
    }

    public static Expression<Func<TEntity, bool>> CombineWithAnd<TEntity>(IEnumerable<Func<TEntity, bool>> predicates)
    {
        Func<TEntity, bool> combined = null;

        foreach (var predicate in predicates)
        {
            combined = combined == null ? predicate : (x => combined(x) && predicate(x));
        }

        if (combined == null)
            return True<TEntity>();

        return x => combined(x);
    }

    public static Expression<Func<TEntity, bool>> CombineWithOr<TEntity>(IEnumerable<Func<TEntity, bool>> predicates)
    {
        Func<TEntity, bool> combined = null;

        foreach (var predicate in predicates)
        {
            combined = combined == null ? predicate : (x => combined(x) || predicate(x));
        }

        if (combined == null)
            return True<TEntity>();

        return x => combined(x);
    }

    public static Expression<Func<TEntity, bool>> CombineWithNot<TEntity>(IEnumerable<Func<TEntity, bool>> predicates)
    {
        Func<TEntity, bool> combined = null;

        foreach (var predicate in predicates)
        {
            combined = combined == null ? predicate : (x => combined(x) && !predicate(x));
        }

        if (combined == null)
            return True<TEntity>();

        return x => combined(x);
    }
}
