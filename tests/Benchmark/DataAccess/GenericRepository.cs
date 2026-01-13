using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Benchmark.Helpers;
using Benchmark.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using Softoverse.EntityFrameworkCore.Specification.Abstraction;
using Softoverse.EntityFrameworkCore.Specification.Helpers;
using Softoverse.EntityFrameworkCore.Specification.Implementation;

namespace Benchmark.DataAccess;

public abstract class GenericRepository<TEntity, TKey>(ApplicationDbContext dbContext)
    : RepositoryBase<TEntity, TKey>(dbContext)
    where TEntity : Entity
{
}

public abstract class RepositoryBase<TEntity, TKey> : IRepositoryBase<TEntity, TKey>
    where TEntity : class
{
    private readonly ApplicationDbContext _dbContext;
    protected readonly DbSet<TEntity> _entity;

    protected RepositoryBase(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _entity = _dbContext.Set<TEntity>();
    }


    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await _entity.FindAsync([id], cancellationToken);
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> expression,
                                         bool asNoTracking = false,
                                         CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? _entity.AsNoTracking() : _entity;
        return await query.FirstOrDefaultAsync(expression, cancellationToken);
    }

    public async Task<TEntity?> GetAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        var query = _entity.ApplySpecification(specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsByAsync(Expression<Func<TEntity, bool>> expression,
                                                  bool asNoTracking = false,
                                                  CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? _entity.AsNoTracking() : _entity;
        return await query.AnyAsync(expression, cancellationToken);
    }

    public virtual async Task<bool> ExistsByAsync(ISpecification<TEntity> specification,
                                                  CancellationToken cancellationToken = default)
    {
        var query = _entity.ApplySpecification(specification);
        return await query.AnyAsync(specification.Criteria ?? (entity => true), cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetAllAsync(ISpecification<TEntity> specification,
                                                         Sortable? sortable = null,
                                                         CancellationToken cancellationToken = default)
    {
        sortable ??= new Sortable();

        var query = _entity.ApplySpecification(specification);

        query = sortable.ApplySorting(query);

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TResult>> GetAllAsync<TResult>(ISpecification<TEntity> specification,
                                                                  Sortable? sortable = null,
                                                                  CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _entity.ApplySpecification(specification);

        sortable ??= new Sortable();

        IQueryable<TResult> newQuery;
        if (specification.ProjectionExpression is null)
        {
            newQuery = query.ProjectTo<TEntity, TResult>();
        }
        else
        {
            newQuery = query.Select(specification.ProjectionExpression!)
                            .OfType<TResult>();
        }

        newQuery = sortable.ApplySorting(newQuery);

        return await newQuery.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? expression,
                                                         Sortable? sortable = null,
                                                         bool asNoTracking = false,
                                                         CancellationToken cancellationToken = default)
    {
        sortable ??= new Sortable();

        var query = asNoTracking ? _entity.AsNoTracking() : _entity;
        var newQuery = query.Where(expression ?? (x => true));

        newQuery = sortable.ApplySorting(newQuery);

        return await newQuery.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<TEntity, bool>>? expression,
                                                                  Sortable? sortable = null,
                                                                  bool asNoTracking = false,
                                                                  CancellationToken cancellationToken = default)
    {
        sortable ??= new Sortable();

        var query = asNoTracking ? _entity.AsNoTracking() : _entity;
        var newQuery = query.Where(expression ?? (x => true)).ProjectTo<TEntity, TResult>();

        newQuery = sortable.ApplySorting(newQuery);

        return await newQuery.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetAllAsync([NotParameterized] FormattableString sql,
                                                         Sortable? sortable = null,
                                                         CancellationToken cancellationToken = default)
    {
        sortable ??= new Sortable();

        var query = _entity.FromSqlInterpolated(sql);

        query = sortable.ApplySorting(query);

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TResult>> GetAllAsync<TResult>([NotParameterized] FormattableString sql,
                                                                  Sortable? sortable = null,
                                                                  CancellationToken cancellationToken = default)
    {
        sortable ??= new Sortable();

        var query = _dbContext.Database.SqlQuery<TResult>(sql);

        query = sortable.ApplySorting(query);

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<PagedData<TEntity>> GetAllPagedAsync(ISpecification<TEntity> specification,
                                                                   Pageable pageable,
                                                                   Sortable? sortable = null,
                                                                   CancellationToken cancellationToken = default)
    {
        sortable ??= new Sortable();

        var query = _entity.ApplySpecification(specification);

        query = sortable.ApplySorting(query);

        return await GetAllPagedAsync(query, pageable, cancellationToken);
    }

    public virtual async Task<PagedData<TResult>> GetAllPagedAsync<TResult>(ISpecification<TEntity> specification,
                                                                            Pageable pageable,
                                                                            Sortable? sortable = null,
                                                                            CancellationToken cancellationToken = default)
    {
        sortable ??= new Sortable();

        IQueryable<TEntity> query = _entity.ApplySpecification(specification);

        IQueryable<TResult> newQuery;
        if (specification.ProjectionExpression is null)
        {
            newQuery = query.ProjectTo<TEntity, TResult>();
        }
        else
        {
            newQuery = query.Select(specification.ProjectionExpression!)
                            .OfType<TResult>();
        }

        newQuery = sortable.ApplySorting(newQuery);

        return await GetAllPagedAsync(newQuery, pageable, cancellationToken);
    }

    public virtual async Task<PagedData<TEntity>> GetAllPagedAsync(Expression<Func<TEntity, bool>>? expression,
                                                                   Pageable pageable,
                                                                   Sortable? sortable = null,
                                                                   bool asNoTracking = false,
                                                                   CancellationToken cancellationToken = default)
    {
        sortable ??= new Sortable();

        var query = asNoTracking ? _entity.AsNoTracking() : _entity;
        var newQuery = query.Where(expression ?? (x => true));

        newQuery = sortable.ApplySorting(newQuery);

        return await GetAllPagedAsync(newQuery, pageable, cancellationToken);
    }

    public virtual async Task<PagedData<TResult>> GetAllPagedAsync<TResult>(Expression<Func<TEntity, bool>>? expression,
                                                                            Pageable pageable,
                                                                            Sortable? sortable = null,
                                                                            bool asNoTracking = false,
                                                                            CancellationToken cancellationToken = default)
    {
        sortable ??= new Sortable();

        var query = asNoTracking ? _entity.AsNoTracking() : _entity;
        var newQuery = query.Where(expression ?? (x => true)).ProjectTo<TEntity, TResult>();

        newQuery = sortable.ApplySorting(newQuery);

        return await GetAllPagedAsync(newQuery, pageable, cancellationToken);
    }

    public virtual async Task<PagedData<TEntity>> GetAllPagedAsync([NotParameterized] FormattableString sql,
                                                                   Pageable pageable,
                                                                   Sortable? sortable = null,
                                                                   CancellationToken cancellationToken = default)
    {
        sortable ??= new Sortable();

        var query = _entity.FromSqlInterpolated(sql);

        query = sortable.ApplySorting(query);

        return await GetAllPagedAsync(query, pageable, cancellationToken);
    }

    public virtual async Task<PagedData<TResult>> GetAllPagedAsync<TResult>([NotParameterized] FormattableString sql,
                                                                            Pageable pageable,
                                                                            Sortable? sortable = null,
                                                                            CancellationToken cancellationToken = default)
    {
        sortable ??= new Sortable();

        var query = _dbContext.Database.SqlQuery<TResult>(sql);

        query = sortable.ApplySorting(query);

        return await GetAllPagedAsync(query, pageable, cancellationToken);
    }

    public static async Task<PagedData<TResult>> GetAllPagedAsync<TResult>(IQueryable<TResult> query,
                                                                            Pageable? pageable,
                                                                            CancellationToken cancellationToken = default)
    {
        pageable ??= new Pageable();

        int totalRecord = await query.CountAsync(cancellationToken);
        int totalPage = 1;

        if (pageable.AsPage)
        {
            totalPage = (int)Math.Ceiling(totalRecord / (decimal)pageable.PageSize);
            query = pageable.ApplyPagination(query);
        }
        else
        {
            pageable.PageSize = totalRecord;
            pageable.PageNumber = 1;
        }

        var data = await query.ToListAsync(cancellationToken);
        return data.ToPagedData(pageable.PageSize, pageable.PageNumber, totalPage, totalRecord);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken
                                           cancellationToken = default)
    {
        await _entity.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities,
                                            CancellationToken cancellationToken = default)
    {
        await _entity.AddRangeAsync(entities, cancellationToken);
    }

    public virtual Task Update(TEntity entity)
    {
        _entity.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task UpdateRange(IEnumerable<TEntity> entities)
    {
        _entity.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public virtual async Task<int> ExecuteUpdateAsync(Expression<Func<TEntity, bool>> expression,
                                                      TEntity model,
                                                      CancellationToken cancellationToken = default)
    {
        await Update(model);
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ExecuteUpdateAsync(Expression<Func<TEntity, bool>> expression,
                                              IDictionary<string, object> model,
                                              CancellationToken cancellationToken = default)
    {
        try
        {
            Action<UpdateSettersBuilder<TEntity>> updateExpression = ExpressionGenerator<TEntity>.BuildUpdateExpression(model);
            return await _entity.Where(expression).ExecuteUpdateAsync(updateExpression, cancellationToken);
        }
        catch (Exception exception)
        {
            return 0;
        }
    }

    public virtual async Task<int> ExecuteUpdateAsync(ISpecification<TEntity> specification,
                                                      TEntity model,
                                                      CancellationToken cancellationToken = default)
    {
        try
        {
            // Apply any filtering if needed
            var query = _entity.ApplySpecification(specification);

            if (specification.ExecuteUpdateExpression is not null)
            {
                return await query.ExecuteUpdateAsync(specification.ExecuteUpdateExpression, cancellationToken);
            }

            if (specification.ExecuteUpdateProperties.Count > 0)
            {
                var updateExpression = ExpressionGenerator<TEntity>.BuildUpdateExpression(specification.ExecuteUpdateProperties, model);
                return await query.ExecuteUpdateAsync(updateExpression, cancellationToken);
            }

            await Update(model);
            return await SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            return 0;
        }
    }

    public async Task<int> ExecuteUpdateAsync(ISpecification<TEntity> specification,
                                              IDictionary<string, object> model,
                                              CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _entity.ApplySpecification(specification);

            Action<UpdateSettersBuilder<TEntity>> updateExpression = ExpressionGenerator<TEntity>.BuildUpdateExpression(model);
            return await query.ExecuteUpdateAsync(updateExpression, cancellationToken);
        }
        catch (Exception exception)
        {
            return 0;
        }
    }

    public virtual Task Remove(TEntity entity)
    {
        _entity.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual Task RemoveRange(IEnumerable<TEntity> entities)
    {
        _entity.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public virtual async Task RemoveByIdAsync(TKey id,
                                              CancellationToken cancellationToken = default)
    {
        var item = await GetByIdAsync(id, cancellationToken);
        if (item is not null)
        {
            _entity.Remove(item);
        }
    }

    public virtual async Task RemoveByIdRangeAsync(Expression<Func<TEntity, bool>> expression,
                                                   bool asNoTracking = false,
                                                   CancellationToken cancellationToken = default)
    {
        var items = await GetAllAsync(expression, null, asNoTracking, cancellationToken: cancellationToken);
        if (items.Count > 0)
        {
            _entity.RemoveRange(items);
        }
    }

    public virtual async Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> expression,
                                                      CancellationToken cancellationToken = default)
    {
        return await _entity.Where(expression).ExecuteDeleteAsync(cancellationToken);
    }

    public virtual async IAsyncEnumerable<TEntity> StreamAllAsync(Expression<Func<TEntity, bool>>? expression = null,
                                                                  bool asNoTracking = false,
                                                                  [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? _entity.AsNoTracking() : _entity;
        var filtered = query.Where(expression ?? (x => true));

        await foreach (var item in filtered.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }

    public virtual async IAsyncEnumerable<TResult> StreamAllAsync<TResult>(Expression<Func<TEntity, bool>>? expression = null,
                                                                           bool asNoTracking = false,
                                                                           [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? _entity.AsNoTracking() : _entity;
        var filtered = query.Where(expression ?? (x => true)).ProjectTo<TEntity, TResult>();

        await foreach (var item in filtered.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }

    public virtual async IAsyncEnumerable<TEntity> StreamAllAsync(ISpecification<TEntity> specification,
                                                                  [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _entity.ApplySpecification(specification);

        await foreach (var item in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }

    public virtual async IAsyncEnumerable<TResult> StreamAllAsync<TResult>(ISpecification<TEntity> specification,
                                                                           [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _entity.ApplySpecification(specification);

        IQueryable<TResult> newQuery;
        if (specification.ProjectionExpression is null)
        {
            newQuery = query.ProjectTo<TEntity, TResult>();
        }
        else
        {
            newQuery = query.Select(specification.ProjectionExpression!)
                            .OfType<TResult>();
        }

        await foreach (var item in newQuery.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }


    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}