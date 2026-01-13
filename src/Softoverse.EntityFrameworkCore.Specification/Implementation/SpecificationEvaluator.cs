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

        // Apply ordering using the new OrderByExpressions collection
        if (specification.OrderByExpressions.Any())
        {
            IOrderedQueryable<TEntity>? orderedQuery = null;
            
            for (int i = 0; i < specification.OrderByExpressions.Count; i++)
            {
                var (keySelector, isDescending) = specification.OrderByExpressions[i];
                
                if (i == 0)
                {
                    // First ordering - use OrderBy or OrderByDescending
                    orderedQuery = isDescending
                        ? queryable.OrderByDescending(keySelector)
                        : queryable.OrderBy(keySelector);
                }
                else
                {
                    // Subsequent orderings - use ThenBy or ThenByDescending
                    orderedQuery = isDescending
                        ? orderedQuery!.ThenByDescending(keySelector)
                        : orderedQuery!.ThenBy(keySelector);
                }
            }
            
            queryable = orderedQuery!;
        }
        // Fallback to legacy single OrderBy properties for backward compatibility
#pragma warning disable CS0618 // Type or member is obsolete
        else if (specification.OrderByExpression is not null)
        {
            queryable = queryable.OrderBy(specification.OrderByExpression);
        }
        else if (specification.OrderByDescendingExpression is not null)
        {
            queryable = queryable.OrderByDescending(specification.OrderByDescendingExpression);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        // Apply split query if specified
        if (specification.AsSplitQuery)
        {
            queryable = queryable.AsSplitQuery();
        }

        return queryable;
    }

    public static IQueryable<TEntity> ApplySpecification<TEntity>(this DbSet<TEntity> query, ISpecification<TEntity> specification) where TEntity : class
    {
        return query.AsQueryable().ApplySpecification(specification);
    }

    public static IQueryable<TEntity> ApplySpecification<TEntity>(this DbContext dbContext, ISpecification<TEntity> specification) where TEntity : class
    {
        return dbContext.Set<TEntity>().ApplySpecification(specification);
    }
}