using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Query;

using Softoverse.EntityFrameworkCore.Specification.Abstraction;

namespace Softoverse.EntityFrameworkCore.Specification.Implementation;

/// <summary>
/// Internal wrapper that enables fluent ThenBy/ThenByDescending syntax with automatic type inference
/// </summary>
internal class OrderableSpecification<TEntity, TProperty> : IOrderableSpecification<TEntity, TProperty>
    where TEntity : class
{
    private readonly Specification<TEntity> _specification;

    public OrderableSpecification(Specification<TEntity> specification)
    {
        _specification = specification;
    }

    /// <summary>
    /// Gets the underlying specification - used by extension methods if needed
    /// </summary>
    public Specification<TEntity> GetSpecification() => _specification;

    public IOrderableSpecification<TEntity, TNextProperty> ThenBy<TNextProperty>(
        Expression<Func<TEntity, TNextProperty>> keySelector)
    {
        _specification.AppendThenBy(keySelector, isDescending: false);
        return new OrderableSpecification<TEntity, TNextProperty>(_specification);
    }

    public IOrderableSpecification<TEntity, TNextProperty> ThenByDescending<TNextProperty>(
        Expression<Func<TEntity, TNextProperty>> keySelector)
    {
        _specification.AppendThenBy(keySelector, isDescending: true);
        return new OrderableSpecification<TEntity, TNextProperty>(_specification);
    }

    // Delegate all ISpecification<TEntity> members to the wrapped specification
    public bool AsSplitQuery
    {
        get => _specification.AsSplitQuery;
        set => _specification.AsSplitQuery = value;
    }

    public bool AsNoTracking
    {
        get => _specification.AsNoTracking;
        set => _specification.AsNoTracking = value;
    }

    public Expression<Func<TEntity, bool>>? Criteria
    {
        get => _specification.Criteria;
        set => _specification.Criteria = value;
    }

    public List<Expression<Func<TEntity, object>>> IncludeExpressions => _specification.IncludeExpressions;
    public List<string> IncludeStrings => _specification.IncludeStrings;
    public List<Func<IQueryable<TEntity>, IQueryable<TEntity>>> IncludeActions => _specification.IncludeActions;

#pragma warning disable CS0618 // Type or member is obsolete
    public Expression<Func<TEntity, object>>? OrderByExpression
    {
        get => _specification.OrderByExpression;
        set => _specification.OrderByExpression = value;
    }

    public Expression<Func<TEntity, object>>? OrderByDescendingExpression
    {
        get => _specification.OrderByDescendingExpression;
        set => _specification.OrderByDescendingExpression = value;
    }
#pragma warning restore CS0618 // Type or member is obsolete

    public List<(Expression<Func<TEntity, object>> KeySelector, bool IsDescending)> OrderByExpressions => _specification.OrderByExpressions;

    public Expression<Func<TEntity, object>>? ProjectionExpression
    {
        get => _specification.ProjectionExpression;
        set => _specification.ProjectionExpression = value;
    }

    public Action<UpdateSettersBuilder<TEntity>>? ExecuteUpdateExpression
    {
        get => _specification.ExecuteUpdateExpression;
        set => _specification.ExecuteUpdateExpression = value;
    }

    public List<Expression<Func<TEntity, object>>> ExecuteUpdateProperties => _specification.ExecuteUpdateProperties;

    public object? PrimaryKey
    {
        get => _specification.PrimaryKey;
        set => _specification.PrimaryKey = value;
    }

    public IIncludableSpecification<TEntity, TNewProperty> AddInclude<TNewProperty>(Expression<Func<TEntity, TNewProperty>> includeExpression)
        => _specification.Include(includeExpression);

    public IIncludableSpecification<TEntity, object> AddIncludeString(string includeString)
        => _specification.IncludeString(includeString);

    public void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
        => _specification.AddOrderBy(orderByExpression);

    public void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
        => _specification.AddOrderByDescending(orderByDescendingExpression);

    public void SetProjection(Expression<Func<TEntity, object>> projectionExpression)
        => _specification.SetProjection(projectionExpression);

    public void SetExecuteUpdateExpression(Action<UpdateSettersBuilder<TEntity>> executeUpdateExpression)
        => _specification.SetExecuteUpdateExpression(executeUpdateExpression);

    public void AddExecuteUpdateProperties(Expression<Func<TEntity, object>> propertySelector)
        => _specification.AddExecuteUpdateProperties(propertySelector);

    public IIncludableSpecification<TEntity, TNewProperty> Include<TNewProperty>(
        Expression<Func<TEntity, TNewProperty>> includeExpression) => _specification.Include(includeExpression);

    public IIncludableSpecification<TEntity, object> IncludeString(string includeString)
        => _specification.IncludeString(includeString);

    public IOrderableSpecification<TEntity, TNewProperty> OrderBy<TNewProperty>(Expression<Func<TEntity, TNewProperty>> keySelector)
        => _specification.OrderBy(keySelector);

    public IOrderableSpecification<TEntity, TNewProperty> OrderByDescending<TNewProperty>(Expression<Func<TEntity, TNewProperty>> keySelector)
        => _specification.OrderByDescending(keySelector);
}

