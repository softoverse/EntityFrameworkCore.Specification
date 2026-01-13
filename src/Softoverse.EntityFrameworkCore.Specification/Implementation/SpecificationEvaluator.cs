using Microsoft.EntityFrameworkCore;

using Softoverse.EntityFrameworkCore.Specification.Abstraction;

namespace Softoverse.EntityFrameworkCore.Specification.Implementation;

public static class SpecificationEvaluator
{
    public static IQueryable<TEntity> ApplySpecification<TEntity>(this IQueryable<TEntity> inputQueryable,
                                                                   ISpecification<TEntity> specification)
        where TEntity : class
    {
        IQueryable<TEntity> queryable = inputQueryable;

        // Apply no tracking if specified
        if (specification.AsNoTracking)
        {
            queryable = queryable.AsNoTracking();
        }

        // Apply all includes via IncludeActions (consolidated from expressions, strings and actions)
        if (specification.IncludeActions.Any())
        {
            queryable = specification.IncludeActions.Aggregate(
                queryable,
                (current, includeAction) => includeAction(current));
        }

        // Apply filtering
        if (specification.Criteria is not null)
        {
            queryable = queryable.Where(specification.Criteria);
        }

        // Apply projection
        if (specification.ProjectionExpression is not null)
        {
            // Note: Select() followed by OfType<TEntity>() might not be what's always intended if TResult != TEntity,
            // but we'll keep the existing logic for now.
            queryable = queryable.Select(specification.ProjectionExpression).OfType<TEntity>();
        }

        // Apply ordering
        if (specification.OrderByExpression is not null)
        {
            queryable = queryable.OrderBy(specification.OrderByExpression);
        }
        else if (specification.OrderByDescendingExpression is not null)
        {
            queryable = queryable.OrderByDescending(specification.OrderByDescendingExpression);
        }

        // Apply split query if specified
        if (specification.AsSplitQuery)
        {
            queryable = queryable.AsSplitQuery();
        }

        return queryable;
    }

    public static IQueryable<TEntity> ApplySpecification<TEntity>(this DbSet<TEntity> query, ISpecification<TEntity> specification) where TEntity : class
    {
        return ApplySpecification(query.AsQueryable(), specification);
    }

    public static IQueryable<TEntity> ApplySpecification<TEntity>(this DbContext dbContext, ISpecification<TEntity> specification) where TEntity : class
    {
        return ApplySpecification(dbContext.Set<TEntity>(), specification);
    }
}