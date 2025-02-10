using Microsoft.EntityFrameworkCore;

using Softoverse.EntityFrameworkCore.Specification.Abstraction;

namespace Softoverse.EntityFrameworkCore.Specification.Implementation;

public static class SpecificationEvaluator
{
    private static async Task<IQueryable<TEntity>> GenerateQuery<TEntity>(DbSet<TEntity> inputQueryable,
                                                                          ISpecification<TEntity> specification,
                                                                          CancellationToken ct = default)
        where TEntity : class
    {
        // Create the initial queryable
        IQueryable<TEntity> queryable = inputQueryable;

        // Apply no tracking if specified
        if (specification.AsNoTracking)
        {
            queryable = queryable.AsNoTracking();
        }

        // Apply includes
        queryable = specification.IncludeExpressions.Aggregate(
                                                               queryable,
                                                               (current, includeExpression) => current.Include(includeExpression));

        queryable = specification.IncludeStrings.Aggregate(
                                                           queryable,
                                                           (current, include) => current.Include(include));

        // Handle primary key search
        if (specification.PrimaryKey is not null)
        {
            inputQueryable = specification.IncludeExpressions.Aggregate(inputQueryable, (current, includeExpression) => current.Include(includeExpression) as DbSet<TEntity>);
            inputQueryable = specification.IncludeStrings.Aggregate(inputQueryable, (current, include) => (DbSet<TEntity>)current.Include(include));

            // If PrimaryKey is set, use Find and return a single-item query
            var entity = await inputQueryable.FindAsync([
                specification.PrimaryKey
            ], cancellationToken: ct);
            return entity != null
                ? new[]
                {
                    entity
                }.AsQueryable()
                : Enumerable.Empty<TEntity>().AsQueryable();
        }

        // Apply filtering
        if (specification.Criteria is not null)
        {
            queryable = queryable.Where(specification.Criteria);
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


    [Obsolete("", true)]
    private static async Task<IQueryable<TEntity>> GenerateQueryOld<TEntity>(DbSet<TEntity> inputQueryable,
                                                                             ISpecification<TEntity> specification,
                                                                             CancellationToken ct = default)
        where TEntity : class
    {
        IQueryable<TEntity> queryable = inputQueryable;
        if (specification.AsNoTracking)
        {
            queryable = inputQueryable.AsNoTracking();
        }

        if (specification.PrimaryKey is not null)
        {
            inputQueryable = specification.IncludeExpressions.Aggregate(inputQueryable, (current, includeExpression) => (DbSet<TEntity>)current.Include(includeExpression));
            inputQueryable = specification.IncludeStrings.Aggregate(inputQueryable, (current, include) => (DbSet<TEntity>)current.Include(include));

            // If PrimaryKey is set, use Find and return a single-item query
            var entity = await inputQueryable.FindAsync(specification.PrimaryKey, ct);
            return entity != null ? new List<TEntity>
            {
                entity
            }.AsQueryable() : Enumerable.Empty<TEntity>().AsQueryable();
        }

        queryable = specification.IncludeExpressions.Aggregate(queryable, (current, includeExpression) => current.Include(includeExpression));
        queryable = specification.IncludeStrings.Aggregate(queryable, (current, include) => current.Include(include));

        if (specification.Criteria is not null)
        {
            queryable = queryable.Where(specification.Criteria);
        }

        if (specification.OrderByExpression is not null)
        {
            queryable = queryable.OrderBy(specification.OrderByExpression);
        }
        else if (specification.OrderByDescendingExpression is not null)
        {
            queryable = queryable.OrderByDescending(specification.OrderByDescendingExpression);
        }

        if (specification.AsSplitQuery)
        {
            queryable = queryable.AsSplitQuery();
        }

        return queryable;
    }


    public static async Task<IQueryable<TEntity>> ApplySpecification<TEntity>(this DbSet<TEntity> query, ISpecification<TEntity> specification, CancellationToken ct = default) where TEntity : class
    {
        return await GenerateQuery(query, specification, ct);
    }

    public static async Task<IQueryable<TEntity>> ApplySpecification<TEntity>(this DbContext dbContext, ISpecification<TEntity> specification, CancellationToken ct = default) where TEntity : class
    {
        return await ApplySpecification(dbContext.Set<TEntity>(), specification, ct);
    }
}