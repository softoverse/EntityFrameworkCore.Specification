namespace Softoverse.Specification.Abstraction;

public interface ISpecificationRequest<TEntity> where TEntity : class
{
    ISpecification<TEntity> GetSpecification(bool asNoTracking = false, bool asSplitQuery = false);
}