using System.Linq.Expressions;

using Softoverse.EntityFrameworkCore.Specification.Abstraction;
using Softoverse.EntityFrameworkCore.Specification.Implementation;
using Softoverse.EntityFrameworkCore.Specification.Tests.Models;

namespace Softoverse.EntityFrameworkCore.Specification.Tests;

public class SpecificationGetSetTests
{
    #region GetQuerySpecification / SetQuerySpecification

    [Fact]
    public void GetQuerySpecification_WhenNoCriteriaSet_ReturnsNull()
    {
        // Arrange
        var spec = new Specification<TestEntity>();

        // Act
        var result = spec.GetQuerySpecification();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetQuerySpecification_WhenCriteriaIsSet_ReturnsCriteria()
    {
        // Arrange
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive
        };

        // Act
        var result = spec.GetQuerySpecification();

        // Assert
        Assert.NotNull(result);
        Assert.Same(spec.Criteria, result);
    }

    [Fact]
    public void SetQuerySpecification_ReplacesCriteria()
    {
        // Arrange
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive
        };
        Expression<Func<TestEntity, bool>> newCriteria = e => e.Age > 18;

        // Act
        spec.SetQuerySpecification(newCriteria);

        // Assert
        Assert.Same(newCriteria, spec.Criteria);
        Assert.Same(newCriteria, spec.GetQuerySpecification());
    }

    [Fact]
    public void SetQuerySpecification_WithNull_ClearsCriteria()
    {
        // Arrange
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive
        };

        // Act
        spec.SetQuerySpecification(null);

        // Assert
        Assert.Null(spec.Criteria);
        Assert.Null(spec.GetQuerySpecification());
    }

    #endregion

    #region GetOrderBySpecifications / SetOrderBySpecifications / ClearOrderBySpecifications

    [Fact]
    public void GetOrderBySpecifications_WhenNoOrderSet_ReturnsEmptyList()
    {
        // Arrange
        var spec = new Specification<TestEntity>();

        // Act
        var result = spec.GetOrderBySpecifications();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetOrderBySpecifications_ReturnsCurrentOrdering()
    {
        // Arrange
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.Name)
            .ThenByDescending(e => e.CreatedAt);

        // Act
        var result = spec.GetOrderBySpecifications();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.False(result[0].IsDescending); // OrderBy = ascending
        Assert.True(result[1].IsDescending);  // ThenByDescending = descending
    }

    [Fact]
    public void GetOrderBySpecifications_ReturnsCopy_NotSameReference()
    {
        // Arrange
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.Name);

        // Act
        var result1 = spec.GetOrderBySpecifications();
        var result2 = spec.GetOrderBySpecifications();

        // Assert — different list instances
        Assert.NotSame(result1, result2);
        // But same content
        Assert.Equal(result1.Count, result2.Count);
    }

    [Fact]
    public void GetOrderBySpecifications_MutatingCopy_DoesNotAffectOriginal()
    {
        // Arrange
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.Name);

        // Act
        var copy = spec.GetOrderBySpecifications();
        copy.Clear(); // mutate the copy

        // Assert — original is unaffected
        var original = spec.GetOrderBySpecifications();
        Assert.Single(original);
    }

    [Fact]
    public void SetOrderBySpecifications_ReplacesExistingOrdering()
    {
        // Arrange
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.Name);

        Expression<Func<TestEntity, object>> ageSelector = e => e.Age;
        Expression<Func<TestEntity, object>> emailSelector = e => e.Email;

        var newOrdering = new List<(Expression<Func<TestEntity, object>> KeySelector, bool IsDescending)>
        {
            (ageSelector, true),
            (emailSelector, false)
        };

        // Act
        spec.SetOrderBySpecifications(newOrdering);

        // Assert
        var result = spec.GetOrderBySpecifications();
        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsDescending);
        Assert.False(result[1].IsDescending);
    }

    [Fact]
    public void ClearOrderBySpecifications_RemovesAllOrdering()
    {
        // Arrange
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.Name)
            .ThenBy(e => e.Age);

        // Act
        spec.ClearOrderBySpecifications();

        // Assert
        var result = spec.GetOrderBySpecifications();
        Assert.Empty(result);
    }

    #endregion

    #region GetIncludeSpecifications / SetIncludeSpecifications / ClearIncludeSpecifications

    [Fact]
    public void GetIncludeSpecifications_WhenNoIncludesSet_ReturnsEmptyLists()
    {
        // Arrange
        var spec = new Specification<TestEntity>();

        // Act
        var (includeExpressions, includeStrings, includeActions) = spec.GetIncludeSpecifications();

        // Assert
        Assert.Empty(includeExpressions);
        Assert.Empty(includeStrings);
        Assert.Empty(includeActions);
    }

    [Fact]
    public void GetIncludeSpecifications_WithExpressionIncludes_ReturnsCopies()
    {
        // Arrange
        var spec = new Specification<TestEntity>();
        spec.Include(e => e.Profile);
        spec.Include(e => e.Orders);

        // Act
        var (includeExpressions, includeStrings, includeActions) = spec.GetIncludeSpecifications();

        // Assert
        Assert.Equal(2, includeExpressions.Count);
        Assert.Equal(2, includeActions.Count);
        Assert.Empty(includeStrings);
    }

    [Fact]
    public void GetIncludeSpecifications_WithStringIncludes_ReturnsCopies()
    {
        // Arrange
        var spec = new Specification<TestEntity>();
        spec.IncludeString("Profile");
        spec.IncludeString("Orders.Items");

        // Act
        var (includeExpressions, includeStrings, includeActions) = spec.GetIncludeSpecifications();

        // Assert
        Assert.Empty(includeExpressions);
        Assert.Equal(2, includeStrings.Count);
        Assert.Contains("Profile", includeStrings);
        Assert.Contains("Orders.Items", includeStrings);
        Assert.Equal(2, includeActions.Count);
    }

    [Fact]
    public void GetIncludeSpecifications_ReturnsCopy_NotSameReference()
    {
        // Arrange
        var spec = new Specification<TestEntity>();
        spec.Include(e => e.Profile);

        // Act
        var result1 = spec.GetIncludeSpecifications();
        var result2 = spec.GetIncludeSpecifications();

        // Assert — different list instances
        Assert.NotSame(result1.IncludeExpressions, result2.IncludeExpressions);
        Assert.NotSame(result1.IncludeStrings, result2.IncludeStrings);
        Assert.NotSame(result1.IncludeActions, result2.IncludeActions);
    }

    [Fact]
    public void GetIncludeSpecifications_MutatingCopy_DoesNotAffectOriginal()
    {
        // Arrange
        var spec = new Specification<TestEntity>();
        spec.Include(e => e.Profile);

        // Act
        var copy = spec.GetIncludeSpecifications();
        copy.IncludeExpressions.Clear();
        copy.IncludeActions.Clear();

        // Assert — original is unaffected
        var original = spec.GetIncludeSpecifications();
        Assert.Single(original.IncludeExpressions);
        Assert.Single(original.IncludeActions);
    }

    [Fact]
    public void SetIncludeSpecifications_ReplacesExistingIncludes()
    {
        // Arrange
        var spec = new Specification<TestEntity>();
        spec.Include(e => e.Profile);
        spec.IncludeString("Orders");

        // Create a new spec to get fresh include data
        var spec2 = new Specification<TestEntity>();
        spec2.Include(e => e.Orders);
        var newIncludes = spec2.GetIncludeSpecifications();

        // Act
        spec.SetIncludeSpecifications(
            newIncludes.IncludeExpressions,
            newIncludes.IncludeStrings,
            newIncludes.IncludeActions);

        // Assert
        var result = spec.GetIncludeSpecifications();
        Assert.Single(result.IncludeExpressions);
        Assert.Empty(result.IncludeStrings);
        Assert.Single(result.IncludeActions);
    }

    [Fact]
    public void ClearIncludeSpecifications_RemovesAllIncludes()
    {
        // Arrange
        var spec = new Specification<TestEntity>();
        spec.Include(e => e.Profile);
        spec.Include(e => e.Orders);
        spec.IncludeString("Orders.Items");

        // Act
        spec.ClearIncludeSpecifications();

        // Assert
        var (includeExpressions, includeStrings, includeActions) = spec.GetIncludeSpecifications();
        Assert.Empty(includeExpressions);
        Assert.Empty(includeStrings);
        Assert.Empty(includeActions);
    }

    #endregion

    #region Interface-level access

    [Fact]
    public void InterfaceMethods_WorkThroughISpecification()
    {
        // Arrange
        ISpecification<TestEntity> spec = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive
        };

        // Act & Assert — query specification
        Assert.NotNull(spec.GetQuerySpecification());
        spec.SetQuerySpecification(e => e.Age > 25);
        Assert.NotNull(spec.GetQuerySpecification());

        // Act & Assert — order by specifications (via interface)
        spec.OrderBy(e => e.Name);
        var ordering = spec.GetOrderBySpecifications();
        Assert.Single(ordering);

        spec.ClearOrderBySpecifications();
        Assert.Empty(spec.GetOrderBySpecifications());

        // Act & Assert — include specifications (via interface)
        spec.Include(e => e.Profile);
        var includes = spec.GetIncludeSpecifications();
        Assert.Single(includes.IncludeExpressions);

        spec.ClearIncludeSpecifications();
        Assert.Empty(spec.GetIncludeSpecifications().IncludeExpressions);
    }

    #endregion
}


