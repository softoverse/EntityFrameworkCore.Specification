using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Query;

namespace Softoverse.EntityFrameworkCore.Specification.Abstraction;

public interface ISpecification<TEntity> : ISpecificationForPrimaryKey where TEntity : class
{
    bool AsSplitQuery { get; }
    bool AsNoTracking { get; }

    public Expression<Func<TEntity, bool>>? Criteria { get; }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public List<Expression<Func<TEntity, object>>> IncludeExpressions { get; }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public List<string> IncludeStrings { get; }

    /// <summary>
    /// List of include expressions with optional filter conditions
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public List<Func<IQueryable<TEntity>, IQueryable<TEntity>>> IncludeActions { get; }

    [Obsolete("Use OrderBy() fluent method instead", false)]
    public Expression<Func<TEntity, object>>? OrderByExpression { get; }

    [Obsolete("Use OrderByDescending() fluent method instead", false)]
    public Expression<Func<TEntity, object>>? OrderByDescendingExpression { get; }

    /// <summary>
    /// List of ordering expressions with direction indicators (true = descending, false = ascending)
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public List<(Expression<Func<TEntity, object>> KeySelector, bool IsDescending)> OrderByExpressions { get; }

    public Expression<Func<TEntity, object>>? ProjectionExpression { get; }
    public Action<UpdateSettersBuilder<TEntity>>? ExecuteUpdateExpression { get; }

    public List<Expression<Func<TEntity, object>>> ExecuteUpdateProperties { get; }

    [Obsolete("Use Include instead", false)]
    IIncludableSpecification<TEntity, TProperty> AddInclude<TProperty>(Expression<Func<TEntity, TProperty>> includeExpression);

    [Obsolete("Use IncludeString instead", false)]
    IIncludableSpecification<TEntity, object> AddIncludeString(string includeString);

    void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression);

    void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression);

    void SetProjection(Expression<Func<TEntity, object>> projectionExpression);

    void SetExecuteUpdateExpression(Action<UpdateSettersBuilder<TEntity>> executeUpdateExpression);

    void AddExecuteUpdateProperties(Expression<Func<TEntity, object>> propertySelector);

    IIncludableSpecification<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty>> includeExpression);

    IIncludableSpecification<TEntity, object> IncludeString(string includeString);

    IOrderableSpecification<TEntity, TProperty> OrderBy<TProperty>(Expression<Func<TEntity, TProperty>> keySelector);

    IOrderableSpecification<TEntity, TProperty> OrderByDescending<TProperty>(Expression<Func<TEntity, TProperty>> keySelector);
}