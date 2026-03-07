using System.Linq.Expressions;

using Softoverse.EntityFrameworkCore.Specification.Abstraction;
using Softoverse.EntityFrameworkCore.Specification.Implementation;
using Softoverse.EntityFrameworkCore.Specification.Tests.Models;

namespace Softoverse.EntityFrameworkCore.Specification.Tests;

/// <summary>
/// Tests that verify IIncludableSpecification and IOrderableSpecification wrappers
/// correctly delegate all ISpecification members to the underlying Specification.
/// All mutations use the concrete Specification; wrappers are verified as read-through delegates.
/// </summary>
public class SpecificationWrapperDelegationTests
{
    // -------------------------------------------------------------------------
    // IIncludableSpecification wrapper — read delegation
    // -------------------------------------------------------------------------

    [Fact]
    public void IncludableSpec_Criteria_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity> { Criteria = e => e.IsActive };
        // Profile is nullable, so use TestProfile? to match the Include return type
        IIncludableSpecification<TestEntity, TestProfile?> includable = spec.Include(e => e.Profile);

        Assert.Same(spec.Criteria, includable.Criteria);
    }

    [Fact]
    public void IncludableSpec_Criteria_ReflectsMutationOnConcreteSpec()
    {
        var spec = new Specification<TestEntity>();
        var includable = spec.Include(e => e.Profile);

        // Mutate via the concrete spec
        spec.Criteria = e => e.Age > 18;

        Assert.NotNull(includable.Criteria);
        Assert.Same(spec.Criteria, includable.Criteria);
    }

    [Fact]
    public void IncludableSpec_AsNoTracking_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity> { AsNoTracking = true };
        var includable = spec.Include(e => e.Profile);

        Assert.True(includable.AsNoTracking);
    }

    [Fact]
    public void IncludableSpec_AsNoTracking_ReflectsMutationOnConcreteSpec()
    {
        var spec = new Specification<TestEntity>();
        var includable = spec.Include(e => e.Profile);

        spec.AsNoTracking = true;

        Assert.True(includable.AsNoTracking);
    }

    [Fact]
    public void IncludableSpec_AsSplitQuery_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity> { AsSplitQuery = true };
        var includable = spec.Include(e => e.Profile);

        Assert.True(includable.AsSplitQuery);
    }

    [Fact]
    public void IncludableSpec_OrderBy_AddsOrderingOnUnderlying()
    {
        var spec = new Specification<TestEntity>();
        var includable = spec.Include(e => e.Profile);

        includable.OrderBy(e => e.Name);

        Assert.Single(spec.GetOrderBySpecifications());
        Assert.False(spec.GetOrderBySpecifications()[0].IsDescending);
    }

    [Fact]
    public void IncludableSpec_OrderByDescending_AddsDescendingOnUnderlying()
    {
        var spec = new Specification<TestEntity>();
        var includable = spec.Include(e => e.Profile);

        includable.OrderByDescending(e => e.CreatedAt);

        Assert.Single(spec.GetOrderBySpecifications());
        Assert.True(spec.GetOrderBySpecifications()[0].IsDescending);
    }

    [Fact]
    public void IncludableSpec_Include_AddsAnotherInclude()
    {
        var spec = new Specification<TestEntity>();
        var includable = spec.Include(e => e.Profile);

        includable.Include(e => e.Orders);

        Assert.Equal(2, spec.GetIncludeSpecifications().IncludeExpressions.Count);
    }

    [Fact]
    public void IncludableSpec_IncludeString_AddsStringInclude()
    {
        var spec = new Specification<TestEntity>();
        var includable = spec.Include(e => e.Profile);

        includable.IncludeString("Orders");

        Assert.Equal("Orders", spec.GetIncludeSpecifications().IncludeStrings[0]);
    }

    [Fact]
    public void IncludableSpec_GetQuerySpecification_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity> { Criteria = e => e.IsActive };
        var includable = spec.Include(e => e.Profile);

        Assert.Same(spec.GetQuerySpecification(), includable.GetQuerySpecification());
    }

    [Fact]
    public void IncludableSpec_SetQuerySpecification_PropagatesBack()
    {
        var spec = new Specification<TestEntity>();
        var includable = spec.Include(e => e.Profile);

        includable.SetQuerySpecification(e => e.Age > 18);

        Assert.NotNull(spec.GetQuerySpecification());
        Assert.Same(spec.GetQuerySpecification(), includable.GetQuerySpecification());
    }

    [Fact]
    public void IncludableSpec_GetOrderBySpecifications_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.Name);
        var includable = spec.Include(e => e.Profile);

        var ordering = includable.GetOrderBySpecifications();

        Assert.Single(ordering);
    }

    [Fact]
    public void IncludableSpec_SetOrderBySpecifications_PropagatesBack()
    {
        var spec = new Specification<TestEntity>();
        var includable = spec.Include(e => e.Profile);

        Expression<Func<TestEntity, object>> ageSelector = e => e.Age;
        includable.SetOrderBySpecifications([(ageSelector, false)]);

        Assert.Single(spec.GetOrderBySpecifications());
    }

    [Fact]
    public void IncludableSpec_ClearOrderBySpecifications_PropagatesBack()
    {
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.Name);
        var includable = spec.Include(e => e.Profile);

        includable.ClearOrderBySpecifications();

        Assert.Empty(spec.GetOrderBySpecifications());
    }

    [Fact]
    public void IncludableSpec_GetIncludeSpecifications_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity>();
        spec.Include(e => e.Profile);
        var includable = spec.Include(e => e.Orders);

        var includes = includable.GetIncludeSpecifications();

        Assert.Equal(2, includes.IncludeExpressions.Count);
    }

    [Fact]
    public void IncludableSpec_ClearIncludeSpecifications_PropagatesBack()
    {
        var spec = new Specification<TestEntity>();
        var includable = spec.Include(e => e.Profile);

        includable.ClearIncludeSpecifications();

        Assert.Empty(spec.GetIncludeSpecifications().IncludeExpressions);
    }

    [Fact]
    public void IncludableSpec_PrimaryKey_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity> { PrimaryKey = 42 };
        var includable = spec.Include(e => e.Profile);

        Assert.Equal(42, includable.PrimaryKey);
    }

    [Fact]
    public void IncludableSpec_PrimaryKey_ReflectsMutationOnConcreteSpec()
    {
        var spec = new Specification<TestEntity>();
        var includable = spec.Include(e => e.Profile);

        spec.PrimaryKey = 99;

        Assert.Equal(99, includable.PrimaryKey);
    }

    [Fact]
    public void IncludableSpec_OrderByExpressions_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.Name);
        ISpecification<TestEntity> includable = spec.Include(e => e.Profile);

        var ordering = includable.OrderByExpressions;

        Assert.Single(ordering);
    }

    [Fact]
    public void IncludableSpec_IncludeActions_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity>();
        ISpecification<TestEntity> includable = spec.Include(e => e.Profile);

        var actions = includable.IncludeActions;

        Assert.Single(actions);
    }

    // -------------------------------------------------------------------------
    // IOrderableSpecification wrapper — read delegation
    // -------------------------------------------------------------------------

    [Fact]
    public void OrderableSpec_Criteria_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity> { Criteria = e => e.IsActive };
        IOrderableSpecification<TestEntity, string> orderable = spec.OrderBy(e => e.Name);

        Assert.Same(spec.Criteria, orderable.Criteria);
    }

    [Fact]
    public void OrderableSpec_Criteria_ReflectsMutationOnConcreteSpec()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        spec.Criteria = e => e.Age > 18;

        Assert.NotNull(orderable.Criteria);
        Assert.Same(spec.Criteria, orderable.Criteria);
    }

    [Fact]
    public void OrderableSpec_AsNoTracking_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity> { AsNoTracking = true };
        var orderable = spec.OrderBy(e => e.Name);

        Assert.True(orderable.AsNoTracking);
    }

    [Fact]
    public void OrderableSpec_AsNoTracking_ReflectsMutationOnConcreteSpec()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        spec.AsNoTracking = true;

        Assert.True(orderable.AsNoTracking);
    }

    [Fact]
    public void OrderableSpec_AsSplitQuery_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity> { AsSplitQuery = true };
        var orderable = spec.OrderBy(e => e.Name);

        Assert.True(orderable.AsSplitQuery);
    }

    [Fact]
    public void OrderableSpec_ThenBy_AddsToUnderlying()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        orderable.ThenBy(e => e.Age);

        Assert.Equal(2, spec.GetOrderBySpecifications().Count);
        Assert.False(spec.GetOrderBySpecifications()[1].IsDescending);
    }

    [Fact]
    public void OrderableSpec_ThenByDescending_AddsToUnderlying()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        orderable.ThenByDescending(e => e.CreatedAt);

        Assert.Equal(2, spec.GetOrderBySpecifications().Count);
        Assert.True(spec.GetOrderBySpecifications()[1].IsDescending);
    }

    [Fact]
    public void OrderableSpec_Include_AddsIncludeOnUnderlying()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        orderable.Include(e => e.Profile);

        Assert.Single(spec.GetIncludeSpecifications().IncludeExpressions);
    }

    [Fact]
    public void OrderableSpec_IncludeString_AddsStringIncludeOnUnderlying()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        orderable.IncludeString("Orders");

        Assert.Equal("Orders", spec.GetIncludeSpecifications().IncludeStrings[0]);
    }

    [Fact]
    public void OrderableSpec_GetQuerySpecification_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity> { Criteria = e => e.IsActive };
        var orderable = spec.OrderBy(e => e.Name);

        Assert.Same(spec.GetQuerySpecification(), orderable.GetQuerySpecification());
    }

    [Fact]
    public void OrderableSpec_SetQuerySpecification_PropagatesBack()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        orderable.SetQuerySpecification(e => e.Age > 18);

        Assert.NotNull(spec.GetQuerySpecification());
        Assert.Same(spec.GetQuerySpecification(), orderable.GetQuerySpecification());
    }

    [Fact]
    public void OrderableSpec_GetOrderBySpecifications_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        var result = orderable.GetOrderBySpecifications();

        Assert.Single(result);
        Assert.False(result[0].IsDescending);
    }

    [Fact]
    public void OrderableSpec_ClearOrderBySpecifications_PropagatesBack()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        orderable.ClearOrderBySpecifications();

        Assert.Empty(spec.GetOrderBySpecifications());
    }

    [Fact]
    public void OrderableSpec_GetIncludeSpecifications_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity>();
        spec.Include(e => e.Profile);
        var orderable = spec.OrderBy(e => e.Name);

        var includes = orderable.GetIncludeSpecifications();

        Assert.Single(includes.IncludeExpressions);
    }

    [Fact]
    public void OrderableSpec_ClearIncludeSpecifications_PropagatesBack()
    {
        var spec = new Specification<TestEntity>();
        spec.Include(e => e.Profile);
        var orderable = spec.OrderBy(e => e.Name);

        orderable.ClearIncludeSpecifications();

        Assert.Empty(spec.GetIncludeSpecifications().IncludeExpressions);
    }

    [Fact]
    public void OrderableSpec_PrimaryKey_DelegatesToUnderlying()
    {
        var spec = new Specification<TestEntity> { PrimaryKey = 7 };
        var orderable = spec.OrderBy(e => e.Name);

        Assert.Equal(7, orderable.PrimaryKey);
    }

    [Fact]
    public void OrderableSpec_PrimaryKey_ReflectsMutationOnConcreteSpec()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        spec.PrimaryKey = 99;

        Assert.Equal(99, orderable.PrimaryKey);
    }

    [Fact]
    public void OrderableSpec_SetOrderBySpecifications_PropagatesBack()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        Expression<Func<TestEntity, object>> emailSelector = e => e.Email;
        orderable.SetOrderBySpecifications([(emailSelector, true)]);

        Assert.Single(spec.GetOrderBySpecifications());
        Assert.True(spec.GetOrderBySpecifications()[0].IsDescending);
    }

    [Fact]
    public void OrderableSpec_SetIncludeSpecifications_PropagatesBack()
    {
        var spec = new Specification<TestEntity>();
        var orderable = spec.OrderBy(e => e.Name);

        // Build fresh include data from a helper spec
        var helper = new Specification<TestEntity>();
        helper.Include(e => e.Profile);
        var (exprs, strings, actions) = helper.GetIncludeSpecifications();

        orderable.SetIncludeSpecifications(exprs, strings, actions);

        Assert.Single(spec.GetIncludeSpecifications().IncludeExpressions);
    }

    // -------------------------------------------------------------------------
    // Fluent chaining — all operations land on the same underlying spec
    // -------------------------------------------------------------------------

    [Fact]
    public void FluentChain_IncludeOrderByThenBy_AllOnSameSpec()
    {
        var spec = new Specification<TestEntity> { Criteria = e => e.IsActive };

        spec.Include(e => e.Profile)
            .OrderBy(e => e.Name)
            .ThenByDescending(e => e.CreatedAt);

        Assert.Single(spec.GetIncludeSpecifications().IncludeExpressions);
        Assert.Equal(2, spec.GetOrderBySpecifications().Count);
        Assert.False(spec.GetOrderBySpecifications()[0].IsDescending);
        Assert.True(spec.GetOrderBySpecifications()[1].IsDescending);
        Assert.NotNull(spec.Criteria);
    }

    [Fact]
    public void FluentChain_OrderByInclude_AllOnSameSpec()
    {
        var spec = new Specification<TestEntity>();

        spec.OrderBy(e => e.Name)
            .Include(e => e.Profile);

        Assert.Single(spec.GetOrderBySpecifications());
        Assert.Single(spec.GetIncludeSpecifications().IncludeExpressions);
    }

    [Fact]
    public void FluentChain_OrderByThenByIncludeString_AllOnSameSpec()
    {
        var spec = new Specification<TestEntity>();

        spec.OrderBy(e => e.Name)
            .ThenBy(e => e.Age)
            .IncludeString("Profile");

        Assert.Equal(2, spec.GetOrderBySpecifications().Count);
        Assert.Equal("Profile", spec.GetIncludeSpecifications().IncludeStrings[0]);
    }

    [Fact]
    public void FluentChain_IncludeStringOrderBy_AllOnSameSpec()
    {
        var spec = new Specification<TestEntity>();

        spec.IncludeString("Profile")
            .OrderBy(e => e.Name)
            .ThenByDescending(e => e.Age);

        Assert.Equal("Profile", spec.GetIncludeSpecifications().IncludeStrings[0]);
        Assert.Equal(2, spec.GetOrderBySpecifications().Count);
    }
}


