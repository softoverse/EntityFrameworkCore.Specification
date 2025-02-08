using System.Linq.Expressions;

namespace Softoverse.Specification.Abstraction;

public interface ISpecification<TEntity> : ISpecificationForPrimaryKey where TEntity : class
{
    bool AsSplitQuery { get; }
    bool AsNoTracking { get; }

    public Expression<Func<TEntity, bool>>? Criteria { get; }

    public List<Expression<Func<TEntity, object>>> IncludeExpressions { get; }
    public List<string> IncludeStrings { get; }

    public Expression<Func<TEntity, object>>? OrderByExpression { get; }

    public Expression<Func<TEntity, object>>? OrderByDescendingExpression { get; }

    public void AddInclude(Expression<Func<TEntity, object>> includeExpression);

    public void AddIncludeString(string includeString);

    public void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression);

    public void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression);
}

public interface ISpecificationForPrimaryKey
{
    object? PrimaryKey { get; }
}

public interface ISpecificationRequest<TEntity> where TEntity : class
{
    ISpecification<TEntity> GetSpecification(bool asNoTracking = false, bool asSplitQuery = false);
}
