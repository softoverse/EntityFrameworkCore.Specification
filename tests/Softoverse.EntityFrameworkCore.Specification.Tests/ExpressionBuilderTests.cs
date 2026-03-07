using System.Linq.Expressions;

using Softoverse.EntityFrameworkCore.Specification.Helpers;
using Softoverse.EntityFrameworkCore.Specification.Tests.Models;

namespace Softoverse.EntityFrameworkCore.Specification.Tests;

/// <summary>
/// Tests for ExpressionBuilder extension methods: And, Or, Not, When,
/// CombineWithAnd, CombineWithOr, CombineWithNot, True, False.
/// </summary>
public class ExpressionBuilderTests
{
    private readonly TestEntity _active18 = new() { IsActive = true, Age = 18, Name = "Alice" };
    private readonly TestEntity _active10 = new() { IsActive = true, Age = 10, Name = "Bob" };
    private readonly TestEntity _inactive18 = new() { IsActive = false, Age = 18, Name = "Charlie" };
    private readonly TestEntity _inactive10 = new() { IsActive = false, Age = 10, Name = "Dave" };

    // -------------------------------------------------------------------------
    // And
    // -------------------------------------------------------------------------

    [Fact]
    public void And_BothTrue_ReturnsTrue()
    {
        Expression<Func<TestEntity, bool>> left = e => e.IsActive;
        Expression<Func<TestEntity, bool>> right = e => e.Age >= 18;

        var combined = left.And(right).Compile();

        Assert.True(combined(_active18));
        Assert.False(combined(_active10));
        Assert.False(combined(_inactive18));
    }

    [Fact]
    public void And_LeftNull_ReturnsRight()
    {
        Expression<Func<TestEntity, bool>> left = null!;
        Expression<Func<TestEntity, bool>> right = e => e.IsActive;

        var result = left.And(right);

        Assert.Same(right, result);
    }

    [Fact]
    public void And_RightNull_ReturnsLeft()
    {
        Expression<Func<TestEntity, bool>> left = e => e.IsActive;
        Expression<Func<TestEntity, bool>> right = null!;

        var result = left.And(right);

        Assert.Same(left, result);
    }

    // -------------------------------------------------------------------------
    // Or
    // -------------------------------------------------------------------------

    [Fact]
    public void Or_EitherTrue_ReturnsTrue()
    {
        Expression<Func<TestEntity, bool>> left = e => e.IsActive;
        Expression<Func<TestEntity, bool>> right = e => e.Age >= 18;

        var combined = left.Or(right).Compile();

        Assert.True(combined(_active18));
        Assert.True(combined(_active10));    // active even though age < 18
        Assert.True(combined(_inactive18));  // not active but age >= 18
        Assert.False(combined(_inactive10)); // neither
    }

    [Fact]
    public void Or_LeftNull_ReturnsRight()
    {
        Expression<Func<TestEntity, bool>> left = null!;
        Expression<Func<TestEntity, bool>> right = e => e.IsActive;

        var result = left.Or(right);

        Assert.Same(right, result);
    }

    [Fact]
    public void Or_RightNull_ReturnsLeft()
    {
        Expression<Func<TestEntity, bool>> left = e => e.IsActive;
        Expression<Func<TestEntity, bool>> right = null!;

        var result = left.Or(right);

        Assert.Same(left, result);
    }

    // -------------------------------------------------------------------------
    // Not
    // -------------------------------------------------------------------------

    [Fact]
    public void Not_InvertsExpression()
    {
        Expression<Func<TestEntity, bool>> expr = e => e.IsActive;

        var negated = expr.Not().Compile();

        Assert.False(negated(_active18));
        Assert.True(negated(_inactive18));
    }

    [Fact]
    public void Not_NullExpression_ReturnsTrueExpression()
    {
        Expression<Func<TestEntity, bool>> expr = null!;

        var result = expr.Not().Compile();

        Assert.True(result(_active18));
        Assert.True(result(_inactive10));
    }

    // -------------------------------------------------------------------------
    // When (conditional application)
    // -------------------------------------------------------------------------

    [Fact]
    public void When_ConditionTrue_ReturnsOriginalExpression()
    {
        Expression<Func<TestEntity, bool>> expr = e => e.IsActive;

        var result = expr.When(true).Compile();

        Assert.True(result(_active18));
        Assert.False(result(_inactive18));
    }

    [Fact]
    public void When_ConditionFalse_ReturnsAlwaysTrue()
    {
        Expression<Func<TestEntity, bool>> expr = e => e.IsActive;

        var result = expr.When(false).Compile();

        Assert.True(result(_active18));
        Assert.True(result(_inactive10)); // always true
    }

    // -------------------------------------------------------------------------
    // True / False
    // -------------------------------------------------------------------------

    [Fact]
    public void True_AlwaysReturnsTrue()
    {
        var expr = ExpressionBuilder.True<TestEntity>().Compile();

        Assert.True(expr(_active18));
        Assert.True(expr(_inactive10));
    }

    [Fact]
    public void False_AlwaysReturnsFalse()
    {
        var expr = ExpressionBuilder.False<TestEntity>().Compile();

        Assert.False(expr(_active18));
        Assert.False(expr(_inactive10));
    }

    // -------------------------------------------------------------------------
    // CombineWithAnd (expressions)
    // -------------------------------------------------------------------------

    [Fact]
    public void CombineWithAnd_AllTrue_ReturnsTrue()
    {
        var expressions = new List<Expression<Func<TestEntity, bool>>>
        {
            e => e.IsActive,
            e => e.Age >= 18,
            e => e.Name.Length > 0
        };

        var combined = expressions.CombineWithAnd().Compile();

        Assert.True(combined(_active18));
        Assert.False(combined(_active10));
    }

    [Fact]
    public void CombineWithAnd_EmptyList_ReturnsAlwaysTrue()
    {
        IEnumerable<Expression<Func<TestEntity, bool>>> expressions = Array.Empty<Expression<Func<TestEntity, bool>>>();

        var combined = expressions.CombineWithAnd().Compile();

        Assert.True(combined(_inactive10));
    }

    [Fact]
    public void CombineWithAnd_NullList_ReturnsAlwaysTrue()
    {
        IEnumerable<Expression<Func<TestEntity, bool>>> expressions = null!;

        var combined = expressions.CombineWithAnd().Compile();

        Assert.True(combined(_inactive10));
    }

    [Fact]
    public void CombineWithAnd_SingleExpression_ReturnsThatExpression()
    {
        var expressions = new List<Expression<Func<TestEntity, bool>>> { e => e.IsActive };

        var combined = expressions.CombineWithAnd().Compile();

        Assert.True(combined(_active18));
        Assert.False(combined(_inactive18));
    }

    // -------------------------------------------------------------------------
    // CombineWithOr (expressions)
    // -------------------------------------------------------------------------

    [Fact]
    public void CombineWithOr_AnyTrue_ReturnsTrue()
    {
        var expressions = new List<Expression<Func<TestEntity, bool>>>
        {
            e => e.IsActive,
            e => e.Age >= 18
        };

        var combined = expressions.CombineWithOr().Compile();

        Assert.True(combined(_active10));
        Assert.True(combined(_inactive18));
        Assert.False(combined(_inactive10));
    }

    [Fact]
    public void CombineWithOr_EmptyList_ReturnsAlwaysTrue()
    {
        IEnumerable<Expression<Func<TestEntity, bool>>> expressions = Array.Empty<Expression<Func<TestEntity, bool>>>();

        var combined = expressions.CombineWithOr().Compile();

        Assert.True(combined(_inactive10));
    }

    [Fact]
    public void CombineWithOr_NullList_ReturnsAlwaysTrue()
    {
        IEnumerable<Expression<Func<TestEntity, bool>>> expressions = null!;

        var combined = expressions.CombineWithOr().Compile();

        Assert.True(combined(_inactive10));
    }

    // -------------------------------------------------------------------------
    // CombineWithNot (expressions)
    // -------------------------------------------------------------------------

    [Fact]
    public void CombineWithNot_SecondNegated()
    {
        // First expression is kept; subsequent ones are negated and ANDed
        var expressions = new List<Expression<Func<TestEntity, bool>>>
        {
            e => e.IsActive,
            e => e.Age >= 18  // will be negated: Age < 18
        };

        var combined = expressions.CombineWithNot().Compile();

        // IsActive AND NOT (Age >= 18) => IsActive AND Age < 18
        Assert.True(combined(_active10));    // active, age < 18
        Assert.False(combined(_active18));   // active, age >= 18 → negated = false
        Assert.False(combined(_inactive10)); // not active
    }

    [Fact]
    public void CombineWithNot_EmptyList_ReturnsAlwaysTrue()
    {
        IEnumerable<Expression<Func<TestEntity, bool>>> expressions = Array.Empty<Expression<Func<TestEntity, bool>>>();

        var combined = expressions.CombineWithNot().Compile();

        Assert.True(combined(_inactive10));
    }

    // -------------------------------------------------------------------------
    // Predicate (Func<T,bool>) overloads
    // -------------------------------------------------------------------------

    [Fact]
    public void And_Predicates_CombinesCorrectly()
    {
        Func<TestEntity, bool> left = e => e.IsActive;
        Func<TestEntity, bool> right = e => e.Age >= 18;

        var combined = left.And(right).Compile();

        Assert.True(combined(_active18));
        Assert.False(combined(_active10));
    }

    [Fact]
    public void Or_Predicates_CombinesCorrectly()
    {
        Func<TestEntity, bool> left = e => e.IsActive;
        Func<TestEntity, bool> right = e => e.Age >= 18;

        var combined = left.Or(right).Compile();

        Assert.True(combined(_active10));
        Assert.True(combined(_inactive18));
        Assert.False(combined(_inactive10));
    }

    [Fact]
    public void Not_Predicate_InvertsResult()
    {
        Func<TestEntity, bool> predicate = e => e.IsActive;

        var negated = predicate.Not().Compile();

        Assert.False(negated(_active18));
        Assert.True(negated(_inactive18));
    }

    [Fact]
    public void When_Predicate_ConditionTrue_AppliesPredicate()
    {
        Func<TestEntity, bool> predicate = e => e.IsActive;

        var result = predicate.When(true).Compile();

        Assert.True(result(_active18));
        Assert.False(result(_inactive18));
    }

    [Fact]
    public void When_Predicate_ConditionFalse_AlwaysTrue()
    {
        Func<TestEntity, bool> predicate = e => e.IsActive;

        var result = predicate.When(false).Compile();

        Assert.True(result(_inactive10));
    }

    [Fact]
    public void CombineWithAnd_Predicates_AllTrue()
    {
        var predicates = new List<Func<TestEntity, bool>>
        {
            e => e.IsActive,
            e => e.Age >= 18
        };

        var combined = predicates.CombineWithAnd().Compile();

        Assert.True(combined(_active18));
        Assert.False(combined(_active10));
    }

    [Fact]
    public void CombineWithOr_Predicates_AnyTrue()
    {
        var predicates = new List<Func<TestEntity, bool>>
        {
            e => e.IsActive,
            e => e.Age >= 18
        };

        var combined = predicates.CombineWithOr().Compile();

        Assert.True(combined(_active10));
        Assert.True(combined(_inactive18));
        Assert.False(combined(_inactive10));
    }

    [Fact]
    public void CombineWithNot_Predicates_SecondNegated()
    {
        var predicates = new List<Func<TestEntity, bool>>
        {
            e => e.IsActive,
            e => e.Age >= 18
        };

        var combined = predicates.CombineWithNot().Compile();

        Assert.True(combined(_active10));
        Assert.False(combined(_active18));
    }

    [Fact]
    public void CombineWithAnd_Predicates_EmptyList_AlwaysTrue()
    {
        IEnumerable<Func<TestEntity, bool>> predicates = Array.Empty<Func<TestEntity, bool>>();

        var combined = predicates.CombineWithAnd().Compile();

        Assert.True(combined(_inactive10));
    }

    [Fact]
    public void CombineWithOr_Predicates_EmptyList_AlwaysTrue()
    {
        IEnumerable<Func<TestEntity, bool>> predicates = Array.Empty<Func<TestEntity, bool>>();

        var combined = predicates.CombineWithOr().Compile();

        Assert.True(combined(_inactive10));
    }

    [Fact]
    public void CombineWithNot_Predicates_EmptyList_AlwaysTrue()
    {
        IEnumerable<Func<TestEntity, bool>> predicates = Array.Empty<Func<TestEntity, bool>>();

        var combined = predicates.CombineWithNot().Compile();

        Assert.True(combined(_inactive10));
    }
}





