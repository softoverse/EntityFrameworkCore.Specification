using System.Linq.Expressions;

namespace Softoverse.EntityFrameworkCore.Specification.Helpers;

public static class ExpressionBuilder
{
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        if (left == null) return right;
        if (right == null) return left;
        if (left == null && right == null) return ExpressionCombiner.True<T>();

        return ExpressionCombiner.And(left!, right);
    }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        if (left == null) return right;
        if (right == null) return left;
        if (left == null && right == null) return ExpressionCombiner.True<T>();

        return ExpressionCombiner.Or(left!, right);
    }

    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
    {
        if (expression == null) return ExpressionCombiner.True<T>();

        return ExpressionCombiner.Not(expression);
    }

    public static Expression<Func<T, bool>> CombineWithAnd<T>(this IEnumerable<Expression<Func<T, bool>>> expressions)
    {
        if (expressions == null || expressions.Count() == 0) return ExpressionCombiner.True<T>();

        return ExpressionCombiner.CombineWithAnd(expressions);
    }

    public static Expression<Func<T, bool>> CombineWithOr<T>(this IEnumerable<Expression<Func<T, bool>>> expressions)
    {
        return expressions == null || expressions.Count() == 0
            ? ExpressionCombiner.True<T>()
            : ExpressionCombiner.CombineWithOr(expressions);
    }

    public static Expression<Func<T, bool>> CombineWithNot<T>(this IEnumerable<Expression<Func<T, bool>>> expressions)
    {
        return expressions == null || expressions.Count() == 0
            ? ExpressionCombiner.True<T>()
            : ExpressionCombiner.CombineWithNot(expressions);
    }

    public static Expression<Func<TEntity, bool>> True<TEntity>()
    {
        return ExpressionCombiner.True<TEntity>();
    }

    public static Expression<Func<TEntity, bool>> False<TEntity>()
    {
        return ExpressionCombiner.False<TEntity>();
    }

    public static Expression<Func<T, bool>> When<T>(this Expression<Func<T, bool>> expression, bool condition)
    {
        return ExpressionCombiner.When(condition, expression);
    }

    // With Predicates
    public static Expression<Func<T, bool>> And<T>(this Func<T, bool> left, Func<T, bool> right)
    {
        if (left == null) return ExpressionCombiner.ToExpression(right);
        if (right == null) return ExpressionCombiner.ToExpression(left);
        if (left == null && right == null) return ExpressionCombiner.True<T>();

        return ExpressionCombiner.And(left!, right);
    }

    public static Expression<Func<T, bool>> Or<T>(this Func<T, bool> left, Func<T, bool> right)
    {
        if (left == null) return ExpressionCombiner.ToExpression(right);
        if (right == null) return ExpressionCombiner.ToExpression(left);
        if (left == null && right == null) return ExpressionCombiner.True<T>();

        return ExpressionCombiner.Or(left!, right);
    }

    public static Expression<Func<T, bool>> Not<T>(this Func<T, bool> predicate)
    {
        if (predicate == null) return ExpressionCombiner.True<T>();

        return ExpressionCombiner.Not(predicate);
    }

    public static Expression<Func<T, bool>> When<T>(this Func<T, bool> predicate, bool condition)
    {
        return ExpressionCombiner.When(condition, predicate);
    }

    public static Expression<Func<T, bool>> CombineWithAnd<T>(this IEnumerable<Func<T, bool>> predicates)
    {
        return predicates == null || predicates.Count() == 0
            ? ExpressionCombiner.True<T>()
            : ExpressionCombiner.CombineWithAnd(predicates);
    }

    public static Expression<Func<T, bool>> CombineWithOr<T>(this IEnumerable<Func<T, bool>> predicates)
    {
        return predicates == null || predicates.Count() == 0
            ? ExpressionCombiner.True<T>()
            : ExpressionCombiner.CombineWithOr(predicates);
    }

    public static Expression<Func<T, bool>> CombineWithNot<T>(this IEnumerable<Func<T, bool>> predicates)
    {
        return predicates == null || predicates.Count() == 0
            ? ExpressionCombiner.True<T>()
            : ExpressionCombiner.CombineWithNot(predicates);
    }

}