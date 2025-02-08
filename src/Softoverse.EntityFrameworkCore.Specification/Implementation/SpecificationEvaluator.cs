

using Microsoft.EntityFrameworkCore;

using Softoverse.Specification.Abstraction;

namespace Softoverse.Specification.Implementation;

public static class SpecificationEvaluator
{
    private static async Task<IQueryable<TEntity>> GenerateQuery<TEntity>(DbSet<TEntity> inputQueryable, ISpecification<TEntity> specification, CancellationToken ct = default) where TEntity : class
    {
        if (specification.AsNoTracking)
        {
            inputQueryable.AsNoTracking();
        }

        if (specification.PrimaryKey is not null)
        {
            inputQueryable = specification.IncludeExpressions.Aggregate(inputQueryable, (current, includeExpression) => (DbSet<TEntity>)current.Include(includeExpression));
            inputQueryable = specification.IncludeStrings.Aggregate(inputQueryable, (current, include) => (DbSet<TEntity>)current.Include(include));

            // If PrimaryKey is set, use Find and return a single-item query
            var entity = await inputQueryable.FindAsync(specification.PrimaryKey, ct);
            return entity != null ? new List<TEntity> { entity }.AsQueryable() : Enumerable.Empty<TEntity>().AsQueryable();
        }

        IQueryable<TEntity> queryable = inputQueryable;

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
            queryable.AsSplitQuery();
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
