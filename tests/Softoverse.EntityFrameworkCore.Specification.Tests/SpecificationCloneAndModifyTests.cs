using System.Linq.Expressions;

using Softoverse.EntityFrameworkCore.Specification.Abstraction;
using Softoverse.EntityFrameworkCore.Specification.Implementation;
using Softoverse.EntityFrameworkCore.Specification.Tests.Models;

namespace Softoverse.EntityFrameworkCore.Specification.Tests;

public class SpecificationCloneAndModifyTests
{
    /// <summary>
    /// Helper method that creates a "clone" of a specification by extracting its parts
    /// and applying them to a new Specification instance.
    /// </summary>
    private static Specification<T> CloneSpecification<T>(ISpecification<T> source) where T : class
    {
        var clone = new Specification<T>
        {
            AsNoTracking = source.AsNoTracking,
            AsSplitQuery = source.AsSplitQuery,
            PrimaryKey = source.PrimaryKey,
            ProjectionExpression = source.ProjectionExpression,
            ExecuteUpdateExpression = source.ExecuteUpdateExpression,
        };

        // Clone query specification
        clone.SetQuerySpecification(source.GetQuerySpecification());

        // Clone order-by specifications
        clone.SetOrderBySpecifications(source.GetOrderBySpecifications());

        // Clone include specifications
        var includes = source.GetIncludeSpecifications();
        clone.SetIncludeSpecifications(includes.IncludeExpressions, includes.IncludeStrings, includes.IncludeActions);

        return clone;
    }

    [Fact]
    public void Clone_PreservesAllParts()
    {
        // Arrange — build an original specification with all parts
        var original = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive && e.Age > 18,
            AsNoTracking = true,
            AsSplitQuery = true,
        };
        original.Include(e => e.Profile);
        original.Include(e => e.Orders);
        original.OrderBy(e => e.Name)
                .ThenByDescending(e => e.CreatedAt);

        // Act
        var clone = CloneSpecification(original);

        // Assert — query
        Assert.NotNull(clone.GetQuerySpecification());
        Assert.Same(original.GetQuerySpecification(), clone.GetQuerySpecification());

        // Assert — flags
        Assert.True(clone.AsNoTracking);
        Assert.True(clone.AsSplitQuery);

        // Assert — ordering
        var originalOrdering = original.GetOrderBySpecifications();
        var cloneOrdering = clone.GetOrderBySpecifications();
        Assert.Equal(originalOrdering.Count, cloneOrdering.Count);
        Assert.False(cloneOrdering[0].IsDescending); // Name ascending
        Assert.True(cloneOrdering[1].IsDescending);  // CreatedAt descending

        // Assert — includes
        var originalIncludes = original.GetIncludeSpecifications();
        var cloneIncludes = clone.GetIncludeSpecifications();
        Assert.Equal(originalIncludes.IncludeExpressions.Count, cloneIncludes.IncludeExpressions.Count);
        Assert.Equal(originalIncludes.IncludeActions.Count, cloneIncludes.IncludeActions.Count);
    }

    [Fact]
    public void Clone_ThenModifyQuery_DoesNotAffectOriginal()
    {
        // Arrange
        var original = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive
        };
        original.OrderBy(e => e.Name);
        original.Include(e => e.Profile);

        // Act — clone and modify the query criteria only
        var clone = CloneSpecification(original);
        clone.SetQuerySpecification(e => e.Age > 25 && e.Email.Contains("test"));

        // Assert — original is unchanged
        Assert.NotNull(original.GetQuerySpecification());
        Assert.NotSame(original.GetQuerySpecification(), clone.GetQuerySpecification());

        // Assert — ordering and includes are preserved in clone
        Assert.Single(clone.GetOrderBySpecifications());
        Assert.Single(clone.GetIncludeSpecifications().IncludeExpressions);
    }

    [Fact]
    public void Clone_ThenModifyOrdering_DoesNotAffectOriginal()
    {
        // Arrange
        var original = new Specification<TestEntity>();
        original.OrderBy(e => e.Name)
                .ThenBy(e => e.Age);
        original.Include(e => e.Profile);

        // Act — clone and change ordering
        var clone = CloneSpecification(original);
        clone.ClearOrderBySpecifications();
        clone.OrderByDescending(e => e.CreatedAt);

        // Assert — original ordering is unchanged
        Assert.Equal(2, original.GetOrderBySpecifications().Count);
        Assert.False(original.GetOrderBySpecifications()[0].IsDescending);
        Assert.False(original.GetOrderBySpecifications()[1].IsDescending);

        // Assert — clone has new ordering
        Assert.Single(clone.GetOrderBySpecifications());
        Assert.True(clone.GetOrderBySpecifications()[0].IsDescending);
    }

    [Fact]
    public void Clone_ThenModifyIncludes_DoesNotAffectOriginal()
    {
        // Arrange
        var original = new Specification<TestEntity>();
        original.Include(e => e.Profile);
        original.Include(e => e.Orders);

        // Act — clone and modify includes
        var clone = CloneSpecification(original);
        clone.ClearIncludeSpecifications();
        clone.Include(e => e.Orders);

        // Assert — original has 2 includes
        Assert.Equal(2, original.GetIncludeSpecifications().IncludeExpressions.Count);
        Assert.Equal(2, original.GetIncludeSpecifications().IncludeActions.Count);

        // Assert — clone has 1 include
        Assert.Single(clone.GetIncludeSpecifications().IncludeExpressions);
        Assert.Single(clone.GetIncludeSpecifications().IncludeActions);
    }

    [Fact]
    public void Clone_ThenClearQuery_OriginalRetainsQuery()
    {
        // Arrange
        var original = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive
        };

        // Act
        var clone = CloneSpecification(original);
        clone.SetQuerySpecification(null);

        // Assert
        Assert.NotNull(original.GetQuerySpecification());
        Assert.Null(clone.GetQuerySpecification());
    }

    [Fact]
    public void Clone_ModifyAllParts_OriginalIsFullyIntact()
    {
        // Arrange — build a fully featured specification
        var original = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive,
            AsNoTracking = true,
        };
        original.Include(e => e.Profile);
        original.Include(e => e.Orders);
        original.IncludeString("Orders.Items");
        original.OrderBy(e => e.Name)
                .ThenByDescending(e => e.CreatedAt);

        // Snapshot original state
        var originalQuery = original.GetQuerySpecification();
        var originalOrderCount = original.GetOrderBySpecifications().Count;
        var originalIncludeExprCount = original.GetIncludeSpecifications().IncludeExpressions.Count;
        var originalIncludeStrCount = original.GetIncludeSpecifications().IncludeStrings.Count;
        var originalIncludeActCount = original.GetIncludeSpecifications().IncludeActions.Count;

        // Act — clone and modify EVERYTHING
        var clone = CloneSpecification(original);
        clone.SetQuerySpecification(e => e.Age > 50);
        clone.ClearOrderBySpecifications();
        clone.OrderBy(e => e.Email);
        clone.ClearIncludeSpecifications();
        clone.AsNoTracking = false;

        // Assert — original is completely untouched
        Assert.Same(originalQuery, original.GetQuerySpecification());
        Assert.Equal(originalOrderCount, original.GetOrderBySpecifications().Count);
        Assert.Equal(originalIncludeExprCount, original.GetIncludeSpecifications().IncludeExpressions.Count);
        Assert.Equal(originalIncludeStrCount, original.GetIncludeSpecifications().IncludeStrings.Count);
        Assert.Equal(originalIncludeActCount, original.GetIncludeSpecifications().IncludeActions.Count);
        Assert.True(original.AsNoTracking);

        // Assert — clone has new values
        Assert.NotSame(originalQuery, clone.GetQuerySpecification());
        Assert.Single(clone.GetOrderBySpecifications());
        Assert.Empty(clone.GetIncludeSpecifications().IncludeExpressions);
        Assert.False(clone.AsNoTracking);
    }

    [Fact]
    public void SetOrderBySpecifications_WithCopiedAndExtendedList_Works()
    {
        // Arrange — scenario: clone ordering and add one more
        var original = new Specification<TestEntity>();
        original.OrderBy(e => e.Name);

        // Act — get ordering, add another, set on new spec
        var clone = CloneSpecification(original);
        var ordering = clone.GetOrderBySpecifications();

        Expression<Func<TestEntity, object>> ageSelector = e => e.Age;
        ordering.Add((ageSelector, true));
        clone.SetOrderBySpecifications(ordering);

        // Assert — clone has 2 orderings, original has 1
        Assert.Single(original.GetOrderBySpecifications());
        Assert.Equal(2, clone.GetOrderBySpecifications().Count);
    }

    [Fact]
    public void SetIncludeSpecifications_WithCopiedAndExtendedLists_Works()
    {
        // Arrange — scenario: clone includes and add one more
        var original = new Specification<TestEntity>();
        original.Include(e => e.Profile);

        // Act — clone, then add another include
        var clone = CloneSpecification(original);
        clone.Include(e => e.Orders);

        // Assert — original has 1, clone has 2
        Assert.Single(original.GetIncludeSpecifications().IncludeExpressions);
        Assert.Equal(2, clone.GetIncludeSpecifications().IncludeExpressions.Count);
    }
}

