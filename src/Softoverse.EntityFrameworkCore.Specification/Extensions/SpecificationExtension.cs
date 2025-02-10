using System.Linq.Expressions;
using System.Numerics;

using Softoverse.EntityFrameworkCore.Specification.Implementation;

namespace Softoverse.EntityFrameworkCore.Specification.Extensions;

public static class SpecificationExtension
{
    public static Expression<Func<TEntity, bool>> ToConditionalExpression<TEntity, TProperty>(this Specification<TEntity> specification,
                                                                                              Expression<Func<TEntity, TProperty>> propertySelector,
                                                                                              TProperty value,
                                                                                              Expression<Func<TEntity, bool>> defaultExpression)
        where TEntity : class
    {
        return Specification<TEntity>.ToConditionalExpressionInternal(propertySelector, value, default!, defaultExpression);
    }

    public static Expression<Func<TEntity, bool>> ToConditionalExpression<TEntity, TProperty>(this Specification<TEntity> specification,
                                                                                              Expression<Func<TEntity, TProperty>> propertySelector,
                                                                                              TProperty value)
        where TProperty : class
        where TEntity : class
    {
        return Specification<TEntity>.ToConditionalExpressionInternal(propertySelector, value, default);
    }

    #region bool

    public static Expression<Func<TEntity, bool>> ToConditionalExpression<TEntity, TProperty>(this Specification<TEntity> specification,
                                                                                              Expression<Func<TEntity, TProperty>> propertySelector,
                                                                                              bool value,
                                                                                              EqualOperation defaultOperation)
        where TProperty : struct, IComparable<bool>
        where TEntity : class
    {
        var newOperation = defaultOperation switch
        {
            EqualOperation.Equal => Operation.Equal,
            EqualOperation.NotEqual => Operation.NotEqual,
            _ => throw new ArgumentException("Invalid operation")
        };

        return Specification<TEntity>.ToConditionalExpressionInternal(propertySelector, value, newOperation);
    }

    #endregion bool

    #region Other Generic

    // public static Expression<Func<TEntity, bool>> ToConditionalExpression<TEntity, TProperty>(this Specification<TEntity> specification,
    //                                                                                           Expression<Func<TEntity, TProperty>> propertySelector,
    //                                                                                           TProperty value,
    //                                                                                           EqualOperation operation)
    //     where TProperty : class
    //     where TEntity : class
    // {
    //     var newOperation = operation switch
    //     {
    //         EqualOperation.Equal => Operation.Equal,
    //         EqualOperation.NotEqual => Operation.NotEqual,
    //         _ => throw new ArgumentException("Invalid operation")
    //     };
    //     return Specification<TEntity>.ToConditionalExpressionInternal(propertySelector, value, newOperation);
    // }
    
    public static Expression<Func<TEntity, bool>> ToConditionalExpression<TEntity, TProperty>(this Specification<TEntity> specification,
                                                                                              Expression<Func<TEntity, TProperty>> propertySelector,
                                                                                              string value,
                                                                                              EqualOperation defaultOperation)
        where TEntity : class
    {
        var newOperation = defaultOperation switch
        {
            EqualOperation.Equal => Operation.Equal,
            EqualOperation.NotEqual => Operation.NotEqual,
            _ => throw new ArgumentException("Invalid operation")
        };
    
        return Specification<TEntity>.ToConditionalExpressionInternal(propertySelector, value, newOperation);
    }

    #endregion Other Generic

    #region numeric

    public static Expression<Func<TEntity, bool>> ToConditionalExpression<TEntity, TProperty>(this Specification<TEntity> specification,
                                                                                              Expression<Func<TEntity, TProperty>> propertySelector,
                                                                                              TProperty value,
                                                                                              CompareOperation defaultOperation)
        where TProperty : struct, IComparable<TProperty>, INumber<TProperty>
        where TEntity : class
    {
        var newOperation = defaultOperation switch
        {
            CompareOperation.Equal => Operation.Equal,
            CompareOperation.NotEqual => Operation.NotEqual,
            CompareOperation.GreaterThan => Operation.GreaterThan,
            CompareOperation.GreaterThanOrEqual => Operation.GreaterThanOrEqual,
            CompareOperation.LessThan => Operation.LessThan,
            CompareOperation.LessThanOrEqual => Operation.LessThanOrEqual,
            _ => throw new ArgumentException("Invalid operation")
        };
        return Specification<TEntity>.ToConditionalExpressionInternal(propertySelector, value, newOperation);
    }

    #endregion numeric
}