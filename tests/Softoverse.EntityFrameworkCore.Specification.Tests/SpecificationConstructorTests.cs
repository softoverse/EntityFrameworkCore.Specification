using System.Linq.Expressions;

using Softoverse.EntityFrameworkCore.Specification.Implementation;
using Softoverse.EntityFrameworkCore.Specification.Tests.Models;

namespace Softoverse.EntityFrameworkCore.Specification.Tests;

/// <summary>
/// Tests for Specification constructors and default property values.
/// </summary>
public class SpecificationConstructorTests
{
    [Fact]
    public void DefaultConstructor_AllPropertiesAreDefault()
    {
        var spec = new Specification<TestEntity>();

        Assert.Null(spec.Criteria);
        Assert.Null(spec.PrimaryKey);
        Assert.False(spec.AsNoTracking);
        Assert.False(spec.AsSplitQuery);
        Assert.Null(spec.ProjectionExpression);
        Assert.Null(spec.ExecuteUpdateExpression);
        Assert.Empty(spec.ExecuteUpdateProperties);
        Assert.Empty(spec.GetOrderBySpecifications());
        Assert.Empty(spec.GetIncludeSpecifications().IncludeExpressions);
        Assert.Empty(spec.GetIncludeSpecifications().IncludeStrings);
        Assert.Empty(spec.GetIncludeSpecifications().IncludeActions);
    }

    [Fact]
    public void PrimaryKeyConstructor_SetsPrimaryKey()
    {
        var spec = new Specification<TestEntity>(42);

        Assert.Equal(42, spec.PrimaryKey);
        Assert.Null(spec.Criteria);
    }

    [Fact]
    public void PrimaryKeyConstructor_WithAsNoTracking_SetsFlag()
    {
        var spec = new Specification<TestEntity>(1, asNoTracking: true);

        Assert.True(spec.AsNoTracking);
        Assert.False(spec.AsSplitQuery);
    }

    [Fact]
    public void PrimaryKeyConstructor_WithAsSplitQuery_SetsFlag()
    {
        var spec = new Specification<TestEntity>(1, asSplitQuery: true);

        Assert.True(spec.AsSplitQuery);
        Assert.False(spec.AsNoTracking);
    }

    [Fact]
    public void PrimaryKeyConstructor_NullPrimaryKey_IsAllowed()
    {
        object? nullKey = null;
        var spec = new Specification<TestEntity>(nullKey);

        Assert.Null(spec.PrimaryKey);
    }

    [Fact]
    public void ExpressionListConstructor_And_CombinesWithAnd()
    {
        var expressions = new List<Expression<Func<TestEntity, bool>>>
        {
            e => e.IsActive,
            e => e.Age > 18
        };

        // CombineType.And is the explicit value being tested
        var spec = new Specification<TestEntity>(expressions, combineType: CombineType.And);

        Assert.NotNull(spec.Criteria);
        var compiled = spec.Criteria!.Compile();
        Assert.True(compiled(new TestEntity { IsActive = true, Age = 25 }));
        Assert.False(compiled(new TestEntity { IsActive = true, Age = 10 }));
        Assert.False(compiled(new TestEntity { IsActive = false, Age = 25 }));
    }

    [Fact]
    public void ExpressionListConstructor_Or_CombinesWithOr()
    {
        var expressions = new List<Expression<Func<TestEntity, bool>>>
        {
            e => e.IsActive,
            e => e.Age > 18
        };

        var spec = new Specification<TestEntity>(expressions, CombineType.Or);

        Assert.NotNull(spec.Criteria);
        var compiled = spec.Criteria!.Compile();
        Assert.True(compiled(new TestEntity { IsActive = true, Age = 10 }));
        Assert.True(compiled(new TestEntity { IsActive = false, Age = 25 }));
        Assert.False(compiled(new TestEntity { IsActive = false, Age = 10 }));
    }

    [Fact]
    public void ExpressionListConstructor_SetsAsNoTrackingAndSplitQuery()
    {
        var expressions = new List<Expression<Func<TestEntity, bool>>> { e => e.IsActive };

        var spec = new Specification<TestEntity>(expressions, combineType: CombineType.And, asNoTracking: true, asSplitQuery: true);

        Assert.True(spec.AsNoTracking);
        Assert.True(spec.AsSplitQuery);
    }

    [Fact]
    public void ExpressionListConstructor_DefaultCombineType_IsAnd()
    {
        var expressions = new List<Expression<Func<TestEntity, bool>>>
        {
            e => e.IsActive,
            e => e.Age > 18
        };

        var spec = new Specification<TestEntity>(expressions);

        var compiled = spec.Criteria!.Compile();
        // Both must be true (AND)
        Assert.False(compiled(new TestEntity { IsActive = true, Age = 10 }));
        Assert.True(compiled(new TestEntity { IsActive = true, Age = 25 }));
    }
}



