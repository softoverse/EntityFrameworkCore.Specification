using System.Linq.Expressions;

namespace Softoverse.EntityFrameworkCore.Specification.Abstraction;

/// <summary>
/// Represents a specification that can have chained includes applied.
/// When TProperty is a collection type (IEnumerable&lt;T&gt;, List&lt;T&gt;, etc.), 
/// ThenInclude works on the element type T.
/// </summary>
/// <typeparam name="TEntity">The root entity type</typeparam>
/// <typeparam name="TProperty">The property type of the last included navigation</typeparam>
public interface IIncludableSpecification<TEntity, TProperty> : ISpecification<TEntity> 
    where TEntity : class
{
    /// <summary>
    /// Chains a ThenInclude to the previous Include.
    /// If the previous Include was a collection, this works on the element type.
    /// </summary>
    IIncludableSpecification<TEntity, TNextProperty> ThenInclude<TNextProperty>(
        Expression<Func<TProperty, TNextProperty>> navigationPropertyPath);

    /// <summary>
    /// ThenInclude for collection navigation properties - works on the element type
    /// </summary>
    IIncludableSpecification<TEntity, TNextProperty> ThenInclude<TPreviousProperty, TNextProperty>(
        Expression<Func<TPreviousProperty, TNextProperty>> navigationPropertyPath);
}

/// <summary>
/// Extension methods for fluent Include/ThenInclude with automatic type inference.
/// Moved to Abstraction namespace to be automatically available with IIncludableSpecification.
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
        if (source is Implementation.IncludableSpecification<TEntity, IEnumerable<TPreviousProperty>> includable)
        {
            includable.GetSpecification().AppendThenInclude(navigationPropertyPath);
            return new Implementation.IncludableSpecification<TEntity, TNextProperty>(includable.GetSpecification());
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
        if (source is Implementation.IncludableSpecification<TEntity, List<TPreviousProperty>> includable)
        {
            includable.GetSpecification().AppendThenInclude(navigationPropertyPath);
            return new Implementation.IncludableSpecification<TEntity, TNextProperty>(includable.GetSpecification());
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
        if (source is Implementation.IncludableSpecification<TEntity, ICollection<TPreviousProperty>> includable)
        {
            includable.GetSpecification().AppendThenInclude(navigationPropertyPath);
            return new Implementation.IncludableSpecification<TEntity, TNextProperty>(includable.GetSpecification());
        }

        throw new InvalidOperationException("Unexpected IIncludableSpecification implementation");
    }
}

