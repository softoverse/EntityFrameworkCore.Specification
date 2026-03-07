﻿using System.Linq.Expressions;

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

    #region Get/Set Specification Parts (for cloning and modification)

    /// <summary>
    /// Gets the current query specification (Criteria) expression.
    /// Returns the filter expression or null if no criteria is set.
    /// </summary>
    Expression<Func<TEntity, bool>>? GetQuerySpecification();

    /// <summary>
    /// Sets (replaces) the query specification (Criteria) expression.
    /// Pass null to clear the criteria.
    /// </summary>
    void SetQuerySpecification(Expression<Func<TEntity, bool>>? criteria);

    /// <summary>
    /// Gets a copy of the current order-by specifications.
    /// Returns a new list containing the ordering expressions and their direction indicators.
    /// </summary>
    List<(Expression<Func<TEntity, object>> KeySelector, bool IsDescending)> GetOrderBySpecifications();

    /// <summary>
    /// Sets (replaces) the order-by specifications with the provided list.
    /// Clears any existing ordering and applies the new list.
    /// </summary>
    void SetOrderBySpecifications(List<(Expression<Func<TEntity, object>> KeySelector, bool IsDescending)> orderBySpecifications);

    /// <summary>
    /// Clears all order-by specifications.
    /// </summary>
    void ClearOrderBySpecifications();

    /// <summary>
    /// Gets a copy of the current include specifications.
    /// Returns a tuple containing copies of IncludeExpressions, IncludeStrings, and IncludeActions lists.
    /// </summary>
    (List<Expression<Func<TEntity, object>>> IncludeExpressions, List<string> IncludeStrings, List<Func<IQueryable<TEntity>, IQueryable<TEntity>>> IncludeActions) GetIncludeSpecifications();

    /// <summary>
    /// Sets (replaces) the include specifications with the provided lists.
    /// Clears any existing includes and applies the new lists.
    /// </summary>
    void SetIncludeSpecifications(
        List<Expression<Func<TEntity, object>>> includeExpressions,
        List<string> includeStrings,
        List<Func<IQueryable<TEntity>, IQueryable<TEntity>>> includeActions);

    /// <summary>
    /// Clears all include specifications (expressions, strings, and actions).
    /// </summary>
    void ClearIncludeSpecifications();

    #endregion
}