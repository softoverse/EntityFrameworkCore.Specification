using System.Linq.Expressions;

namespace Softoverse.EntityFrameworkCore.Specification.Abstraction;

/// <summary>
/// Represents a specification that can have chained ordering applied.
/// Enables fluent ThenBy and ThenByDescending syntax with automatic type inference.
/// </summary>
/// <typeparam name="TEntity">The root entity type</typeparam>
/// <typeparam name="TProperty">The property type of the last ordering expression</typeparam>
public interface IOrderableSpecification<TEntity, TProperty> : ISpecification<TEntity> 
    where TEntity : class
{
    /// <summary>
    /// Chains a ThenBy to the previous OrderBy or ThenBy.
    /// Applies ascending secondary sort on the specified property.
    /// </summary>
    IOrderableSpecification<TEntity, TNextProperty> ThenBy<TNextProperty>(
        Expression<Func<TEntity, TNextProperty>> keySelector);

    /// <summary>
    /// Chains a ThenByDescending to the previous OrderBy or ThenBy.
    /// Applies descending secondary sort on the specified property.
    /// </summary>
    IOrderableSpecification<TEntity, TNextProperty> ThenByDescending<TNextProperty>(
        Expression<Func<TEntity, TNextProperty>> keySelector);
}

