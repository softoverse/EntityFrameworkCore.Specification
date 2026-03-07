using Softoverse.EntityFrameworkCore.Specification.Abstraction;
using Softoverse.EntityFrameworkCore.Specification.Implementation;
using Softoverse.EntityFrameworkCore.Specification.Tests.Models;

namespace Softoverse.EntityFrameworkCore.Specification.Tests;

/// <summary>
/// Tests for Include, IncludeString, and ThenInclude fluent API.
/// </summary>
public class SpecificationIncludeTests
{
    [Fact]
    public void Include_AddsToIncludeExpressions()
    {
        var spec = new Specification<TestEntity>();

        spec.Include(e => e.Profile);

        var includes = spec.GetIncludeSpecifications();
        Assert.Single(includes.IncludeExpressions);
        Assert.Empty(includes.IncludeStrings);
        Assert.Single(includes.IncludeActions);
    }

    [Fact]
    public void Include_MultipleIncludes_AllAdded()
    {
        var spec = new Specification<TestEntity>();

        spec.Include(e => e.Profile);
        spec.Include(e => e.Orders);

        var includes = spec.GetIncludeSpecifications();
        Assert.Equal(2, includes.IncludeExpressions.Count);
        Assert.Equal(2, includes.IncludeActions.Count);
    }

    [Fact]
    public void IncludeString_AddsToIncludeStrings()
    {
        var spec = new Specification<TestEntity>();

        spec.IncludeString("Profile");

        var includes = spec.GetIncludeSpecifications();
        Assert.Empty(includes.IncludeExpressions);
        Assert.Single(includes.IncludeStrings);
        Assert.Equal("Profile", includes.IncludeStrings[0]);
        Assert.Single(includes.IncludeActions);
    }

    [Fact]
    public void IncludeString_MultipleStrings_AllAdded()
    {
        var spec = new Specification<TestEntity>();

        spec.IncludeString("Profile");
        spec.IncludeString("Orders.Items");

        var includes = spec.GetIncludeSpecifications();
        Assert.Equal(2, includes.IncludeStrings.Count);
        Assert.Equal(2, includes.IncludeActions.Count);
        Assert.Contains("Profile", includes.IncludeStrings);
        Assert.Contains("Orders.Items", includes.IncludeStrings);
    }

    [Fact]
    public void Include_ReturnsIIncludableSpecification_ForChaining()
    {
        var spec = new Specification<TestEntity>();

        var result = spec.Include(e => e.Orders);

        Assert.NotNull(result);
    }

    [Fact]
    public void Include_ThenInclude_AddsChainedInclude()
    {
        var spec = new Specification<TestEntity>();

        spec.Include(e => e.Orders)
            .ThenInclude<TestEntity, TestOrder, List<TestOrderItem>>(o => o.Items);

        // ThenInclude modifies the last IncludeAction, count stays at 1
        var includes = spec.GetIncludeSpecifications();
        Assert.Single(includes.IncludeActions);
    }

    [Fact]
    public void IncludeString_ReturnsIIncludableSpecification_ForChaining()
    {
        var spec = new Specification<TestEntity>();

        var result = spec.IncludeString("Profile");

        Assert.NotNull(result);
    }

    [Fact]
    public void Include_MixedWithIncludeString_BothTracked()
    {
        var spec = new Specification<TestEntity>();

        spec.Include(e => e.Profile);
        spec.IncludeString("Orders");

        var includes = spec.GetIncludeSpecifications();
        Assert.Single(includes.IncludeExpressions);
        Assert.Single(includes.IncludeStrings);
        Assert.Equal(2, includes.IncludeActions.Count);
    }

    [Fact]
    public void Include_CollectionNavigation_AddsCorrectly()
    {
        var spec = new Specification<TestEntity>();

        spec.Include(e => e.Orders);

        var includes = spec.GetIncludeSpecifications();
        Assert.Single(includes.IncludeExpressions);
        Assert.Single(includes.IncludeActions);
    }

    [Fact]
    public void Include_ThenInclude_OnCollection_Works()
    {
        var spec = new Specification<TestEntity>();

        spec.Include(e => e.Orders)
            .ThenInclude<TestEntity, TestOrder, List<TestOrderItem>>(o => o.Items);

        var includes = spec.GetIncludeSpecifications();
        Assert.Single(includes.IncludeActions);
    }

    [Fact]
    public void MultipleIncludes_ThenInclude_EachMaintainsSeparateAction()
    {
        var spec = new Specification<TestEntity>();

        spec.Include(e => e.Profile);
        spec.Include(e => e.Orders)
            .ThenInclude<TestEntity, TestOrder, List<TestOrderItem>>(o => o.Items);

        var includes = spec.GetIncludeSpecifications();
        Assert.Equal(2, includes.IncludeExpressions.Count);
        Assert.Equal(2, includes.IncludeActions.Count);
    }

    [Fact]
    [Obsolete("Testing obsolete AddInclude method")]
    public void AddInclude_Obsolete_StillWorks()
    {
#pragma warning disable CS0618
        var spec = new Specification<TestEntity>();
        spec.AddInclude(e => e.Profile);
#pragma warning restore CS0618

        var includes = spec.GetIncludeSpecifications();
        Assert.Single(includes.IncludeExpressions);
    }

    [Fact]
    [Obsolete("Testing obsolete AddIncludeString method")]
    public void AddIncludeString_Obsolete_StillWorks()
    {
#pragma warning disable CS0618
        var spec = new Specification<TestEntity>();
        spec.AddIncludeString("Profile");
#pragma warning restore CS0618

        var includes = spec.GetIncludeSpecifications();
        Assert.Single(includes.IncludeStrings);
        Assert.Equal("Profile", includes.IncludeStrings[0]);
    }
}




