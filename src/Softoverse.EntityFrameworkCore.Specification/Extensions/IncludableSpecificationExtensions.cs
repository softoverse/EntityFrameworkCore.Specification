using System.Linq.Expressions;
using Softoverse.EntityFrameworkCore.Specification.Abstraction;
using Softoverse.EntityFrameworkCore.Specification.Implementation;

namespace Softoverse.EntityFrameworkCore.Specification.Extensions;

/// <summary>
/// Extension methods for fluent Include/ThenInclude with automatic type inference
/// </summary>
public static class IncludableSpecificationExtensions
{
    /// <summary>
    /// ThenInclude for collection navigation properties - works on the element type
    /// </summary>
    public static IIncludableSpecification<TEntity, TNextProperty> ThenInclude<TEntity, TPreviousProperty, TNextProperty>(
        this IIncludableSpecification<TEntity, IEnumerable<TPreviousProperty>> source,
        Expression<Func<TPreviousProperty, TNextProperty>> navigationPropertyPath)
        where TEntity : class
    {
        // Get the underlying IncludableSpecification and call AppendThenInclude directly
        if (source is IncludableSpecification<TEntity, IEnumerable<TPreviousProperty>> includable)
        {
            includable.GetSpecification().AppendThenInclude(navigationPropertyPath);
            return new IncludableSpecification<TEntity, TNextProperty>(includable.GetSpecification());
        }
        
        throw new InvalidOperationException("Unexpected IIncludableSpecification implementation");
    }
    
    /// <summary>
    /// ThenInclude for collection navigation properties (List) - works on the element type
    /// </summary>
    public static IIncludableSpecification<TEntity, TNextProperty> ThenInclude<TEntity, TPreviousProperty, TNextProperty>(
        this IIncludableSpecification<TEntity, List<TPreviousProperty>> source,
        Expression<Func<TPreviousProperty, TNextProperty>> navigationPropertyPath)
        where TEntity : class
    {
        // Get the underlying IncludableSpecification and call AppendThenInclude directly
        if (source is IncludableSpecification<TEntity, List<TPreviousProperty>> includable)
        {
            includable.GetSpecification().AppendThenInclude(navigationPropertyPath);
            return new IncludableSpecification<TEntity, TNextProperty>(includable.GetSpecification());
        }
        
        throw new InvalidOperationException("Unexpected IIncludableSpecification implementation");
    }
    
    /// <summary>
    /// ThenInclude for collection navigation properties (ICollection) - works on the element type
    /// </summary>
    public static IIncludableSpecification<TEntity, TNextProperty> ThenInclude<TEntity, TPreviousProperty, TNextProperty>(
        this IIncludableSpecification<TEntity, ICollection<TPreviousProperty>> source,
        Expression<Func<TPreviousProperty, TNextProperty>> navigationPropertyPath)
        where TEntity : class
    {
        // Get the underlying IncludableSpecification and call AppendThenInclude directly
        if (source is IncludableSpecification<TEntity, ICollection<TPreviousProperty>> includable)
        {
            includable.GetSpecification().AppendThenInclude(navigationPropertyPath);
            return new IncludableSpecification<TEntity, TNextProperty>(includable.GetSpecification());
        }
        
        throw new InvalidOperationException("Unexpected IIncludableSpecification implementation");
    }
}

