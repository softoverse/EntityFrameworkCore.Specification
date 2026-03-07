using Softoverse.EntityFrameworkCore.Specification.Implementation;
using Softoverse.EntityFrameworkCore.Specification.Tests.Models;

namespace Softoverse.EntityFrameworkCore.Specification.Tests;

/// <summary>
/// Tests for OrderBy, OrderByDescending, ThenBy, ThenByDescending fluent API.
/// </summary>
public class SpecificationOrderingTests
{
    [Fact]
    public void OrderBy_AddsAscendingOrder()
    {
        var spec = new Specification<TestEntity>();

        spec.OrderBy(e => e.Name);

        var ordering = spec.GetOrderBySpecifications();
        Assert.Single(ordering);
        Assert.False(ordering[0].IsDescending);
    }

    [Fact]
    public void OrderByDescending_AddsDescendingOrder()
    {
        var spec = new Specification<TestEntity>();

        spec.OrderByDescending(e => e.CreatedAt);

        var ordering = spec.GetOrderBySpecifications();
        Assert.Single(ordering);
        Assert.True(ordering[0].IsDescending);
    }

    [Fact]
    public void OrderBy_ClearsExistingOrdering()
    {
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.Name);

        // Second OrderBy should clear first
        spec.OrderBy(e => e.Age);

        var ordering = spec.GetOrderBySpecifications();
        Assert.Single(ordering);
    }

    [Fact]
    public void OrderByDescending_ClearsExistingOrdering()
    {
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.Name);

        spec.OrderByDescending(e => e.CreatedAt);

        var ordering = spec.GetOrderBySpecifications();
        Assert.Single(ordering);
        Assert.True(ordering[0].IsDescending);
    }

    [Fact]
    public void OrderBy_ThenBy_AddsTwoLevels()
    {
        var spec = new Specification<TestEntity>();

        spec.OrderBy(e => e.Name)
            .ThenBy(e => e.Age);

        var ordering = spec.GetOrderBySpecifications();
        Assert.Equal(2, ordering.Count);
        Assert.False(ordering[0].IsDescending);
        Assert.False(ordering[1].IsDescending);
    }

    [Fact]
    public void OrderBy_ThenByDescending_SecondLevelIsDescending()
    {
        var spec = new Specification<TestEntity>();

        spec.OrderBy(e => e.Name)
            .ThenByDescending(e => e.CreatedAt);

        var ordering = spec.GetOrderBySpecifications();
        Assert.Equal(2, ordering.Count);
        Assert.False(ordering[0].IsDescending);
        Assert.True(ordering[1].IsDescending);
    }

    [Fact]
    public void OrderByDescending_ThenBy_FirstDescSecondAsc()
    {
        var spec = new Specification<TestEntity>();

        spec.OrderByDescending(e => e.CreatedAt)
            .ThenBy(e => e.Name);

        var ordering = spec.GetOrderBySpecifications();
        Assert.Equal(2, ordering.Count);
        Assert.True(ordering[0].IsDescending);
        Assert.False(ordering[1].IsDescending);
    }

    [Fact]
    public void OrderBy_ThreeLevels_AllAdded()
    {
        var spec = new Specification<TestEntity>();

        spec.OrderBy(e => e.IsActive)
            .ThenByDescending(e => e.CreatedAt)
            .ThenBy(e => e.Name);

        var ordering = spec.GetOrderBySpecifications();
        Assert.Equal(3, ordering.Count);
        Assert.False(ordering[0].IsDescending);
        Assert.True(ordering[1].IsDescending);
        Assert.False(ordering[2].IsDescending);
    }

    [Fact]
    public void OrderBy_ReturnsIOrderableSpecification_ForChaining()
    {
        var spec = new Specification<TestEntity>();

        var result = spec.OrderBy(e => e.Name);

        Assert.NotNull(result);
    }

    [Fact]
    public void OrderByDescending_ReturnsIOrderableSpecification_ForChaining()
    {
        var spec = new Specification<TestEntity>();

        var result = spec.OrderByDescending(e => e.Name);

        Assert.NotNull(result);
    }

    [Fact]
    public void ThenBy_OnOrderableResult_Chains()
    {
        var spec = new Specification<TestEntity>();

        var orderable = spec.OrderBy(e => e.Name);
        var result = orderable.ThenBy(e => e.Age);

        Assert.NotNull(result);
        Assert.Equal(2, spec.GetOrderBySpecifications().Count);
    }

    [Fact]
    public void ThenByDescending_OnOrderableResult_Chains()
    {
        var spec = new Specification<TestEntity>();

        var result = spec.OrderBy(e => e.Name)
                         .ThenByDescending(e => e.CreatedAt)
                         .ThenBy(e => e.Age);

        Assert.NotNull(result);
        Assert.Equal(3, spec.GetOrderBySpecifications().Count);
    }

    [Fact]
    public void OrderByWithoutThen_NoOrderSet_ReturnsEmpty()
    {
        var spec = new Specification<TestEntity>();

        var ordering = spec.GetOrderBySpecifications();

        Assert.Empty(ordering);
    }

    [Fact]
    [Obsolete("Testing obsolete AddOrderBy")]
    public void AddOrderBy_Obsolete_StillSetsExpression()
    {
#pragma warning disable CS0618
        var spec = new Specification<TestEntity>();
        spec.AddOrderBy(e => e.Name);
        Assert.NotNull(spec.OrderByExpression);
#pragma warning restore CS0618
    }

    [Fact]
    [Obsolete("Testing obsolete AddOrderByDescending")]
    public void AddOrderByDescending_Obsolete_StillSetsExpression()
    {
#pragma warning disable CS0618
        var spec = new Specification<TestEntity>();
        spec.AddOrderByDescending(e => e.Name);
        Assert.NotNull(spec.OrderByDescendingExpression);
#pragma warning restore CS0618
    }
}

