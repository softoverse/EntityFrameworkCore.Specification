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
}

