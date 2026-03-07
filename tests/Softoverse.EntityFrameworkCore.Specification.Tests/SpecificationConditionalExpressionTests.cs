using Softoverse.EntityFrameworkCore.Specification.Implementation;
using Softoverse.EntityFrameworkCore.Specification.Tests.Models;

namespace Softoverse.EntityFrameworkCore.Specification.Tests;

/// <summary>
/// Tests for ToConditionalExpression static methods: string query operators,
/// numeric comparisons, equal operations, and bool comparisons.
/// </summary>
public class SpecificationConditionalExpressionTests
{
    // -------------------------------------------------------------------------
    // String operators
    // -------------------------------------------------------------------------

    [Fact]
    public void ToConditionalExpression_StringEq_MatchesExactValue()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "eq:Alice", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "Alice" }));
        Assert.False(compiled(new TestEntity { Name = "Bob" }));
    }

    [Fact]
    public void ToConditionalExpression_StringNe_ExcludesValue()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "ne:Alice", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.False(compiled(new TestEntity { Name = "Alice" }));
        Assert.True(compiled(new TestEntity { Name = "Bob" }));
    }

    [Fact]
    public void ToConditionalExpression_Like_ContainsSubstring()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "like:Ali", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "Alice" }));
        Assert.False(compiled(new TestEntity { Name = "Bob" }));
    }

    [Fact]
    public void ToConditionalExpression_LikeCi_CaseInsensitiveContains()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "likeci:alice", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "Alice" }));
        Assert.True(compiled(new TestEntity { Name = "ALICE" }));
        Assert.False(compiled(new TestEntity { Name = "Bob" }));
    }

    [Fact]
    public void ToConditionalExpression_EqCi_CaseInsensitiveEqual()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "eqci:alice", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "Alice" }));
        Assert.True(compiled(new TestEntity { Name = "ALICE" }));
        Assert.False(compiled(new TestEntity { Name = "Bob" }));
    }

    [Fact]
    public void ToConditionalExpression_In_MatchesAnyInList()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "in:Alice,Bob,Charlie", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "Alice" }));
        Assert.True(compiled(new TestEntity { Name = "Bob" }));
        Assert.False(compiled(new TestEntity { Name = "Dave" }));
    }

    [Fact]
    public void ToConditionalExpression_Nin_ExcludesListMembers()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "nin:Alice,Bob", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.False(compiled(new TestEntity { Name = "Alice" }));
        Assert.False(compiled(new TestEntity { Name = "Bob" }));
        Assert.True(compiled(new TestEntity { Name = "Charlie" }));
    }

    [Fact]
    public void ToConditionalExpression_InCi_CaseInsensitiveInList()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "inci:alice,bob", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "Alice" }));
        Assert.True(compiled(new TestEntity { Name = "BOB" }));
        Assert.False(compiled(new TestEntity { Name = "Charlie" }));
    }

    [Fact]
    public void ToConditionalExpression_NinCi_CaseInsensitiveNotInList()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "ninci:alice,bob", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.False(compiled(new TestEntity { Name = "Alice" }));
        Assert.False(compiled(new TestEntity { Name = "BOB" }));
        Assert.True(compiled(new TestEntity { Name = "Charlie" }));
    }

    [Fact]
    public void ToConditionalExpression_InLike_MatchesAnySubstring()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "inlike:Ali,Bob", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "Alice" }));
        Assert.True(compiled(new TestEntity { Name = "Bobby" }));
        Assert.False(compiled(new TestEntity { Name = "Charlie" }));
    }

    [Fact]
    public void ToConditionalExpression_NinLike_ExcludesSubstringMatches()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "ninlike:Ali,Bob", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.False(compiled(new TestEntity { Name = "Alice" }));
        Assert.False(compiled(new TestEntity { Name = "Bobby" }));
        Assert.True(compiled(new TestEntity { Name = "Charlie" }));
    }

    [Fact]
    public void ToConditionalExpression_InLikeCi_CaseInsensitiveSubstringMatch()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "inlikeci:ali,bob", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "Alice" }));
        Assert.True(compiled(new TestEntity { Name = "BOBby" }));
        Assert.False(compiled(new TestEntity { Name = "Charlie" }));
    }

    [Fact]
    public void ToConditionalExpression_NinLikeCi_CaseInsensitiveSubstringExclude()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "ninlikeci:ali,bob", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.False(compiled(new TestEntity { Name = "Alice" }));
        Assert.False(compiled(new TestEntity { Name = "BOBby" }));
        Assert.True(compiled(new TestEntity { Name = "Charlie" }));
    }

    // -------------------------------------------------------------------------
    // Non-query-string: falls back to default expression / operation
    // -------------------------------------------------------------------------

    [Fact]
    public void ToConditionalExpression_PlainStringValue_UsesDefaultExpression()
    {
        var defaultExpr = (System.Linq.Expressions.Expression<Func<TestEntity, bool>>)(e => e.Name == "Alice");

        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "Alice", defaultExpr);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "Alice" }));
        Assert.False(compiled(new TestEntity { Name = "Bob" }));
    }

    [Fact]
    public void ToConditionalExpression_StringWithEqualOperation_UsesDefaultEqual()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "Alice", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "Alice" }));
        Assert.False(compiled(new TestEntity { Name = "Bob" }));
    }

    [Fact]
    public void ToConditionalExpression_StringWithNotEqualOperation_UsesDefaultNotEqual()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Name, "Alice", EqualOperation.NotEqual);

        var compiled = expr.Compile();
        Assert.False(compiled(new TestEntity { Name = "Alice" }));
        Assert.True(compiled(new TestEntity { Name = "Bob" }));
    }

    // -------------------------------------------------------------------------
    // Numeric comparisons
    // -------------------------------------------------------------------------

    [Fact]
    public void ToConditionalExpression_IntGt_GreaterThan()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, "gt:18", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 25 }));
        Assert.False(compiled(new TestEntity { Age = 18 }));
        Assert.False(compiled(new TestEntity { Age = 10 }));
    }

    [Fact]
    public void ToConditionalExpression_IntGte_GreaterThanOrEqual()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, "gte:18", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 18 }));
        Assert.True(compiled(new TestEntity { Age = 25 }));
        Assert.False(compiled(new TestEntity { Age = 10 }));
    }

    [Fact]
    public void ToConditionalExpression_IntLt_LessThan()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, "lt:18", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 10 }));
        Assert.False(compiled(new TestEntity { Age = 18 }));
        Assert.False(compiled(new TestEntity { Age = 25 }));
    }

    [Fact]
    public void ToConditionalExpression_IntLte_LessThanOrEqual()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, "lte:18", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 18 }));
        Assert.True(compiled(new TestEntity { Age = 10 }));
        Assert.False(compiled(new TestEntity { Age = 25 }));
    }

    [Fact]
    public void ToConditionalExpression_IntEq_Equal()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, "eq:25", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 25 }));
        Assert.False(compiled(new TestEntity { Age = 10 }));
    }

    [Fact]
    public void ToConditionalExpression_IntNe_NotEqual()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, "ne:25", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.False(compiled(new TestEntity { Age = 25 }));
        Assert.True(compiled(new TestEntity { Age = 10 }));
    }

    [Fact]
    public void ToConditionalExpression_IntRange_InclusiveBothEnds()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, "range:18,30", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 18 }));
        Assert.True(compiled(new TestEntity { Age = 25 }));
        Assert.True(compiled(new TestEntity { Age = 30 }));
        Assert.False(compiled(new TestEntity { Age = 17 }));
        Assert.False(compiled(new TestEntity { Age = 31 }));
    }

    [Fact]
    public void ToConditionalExpression_IntIn_MatchesList()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, "in:10,20,30", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 10 }));
        Assert.True(compiled(new TestEntity { Age = 20 }));
        Assert.False(compiled(new TestEntity { Age = 15 }));
    }

    [Fact]
    public void ToConditionalExpression_IntNin_ExcludesFromList()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, "nin:10,20", EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.False(compiled(new TestEntity { Age = 10 }));
        Assert.False(compiled(new TestEntity { Age = 20 }));
        Assert.True(compiled(new TestEntity { Age = 30 }));
    }

    // -------------------------------------------------------------------------
    // Numeric CompareOperation overload
    // -------------------------------------------------------------------------

    [Fact]
    public void ToConditionalExpression_IntCompareOperation_GreaterThan()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, 18, CompareOperation.GreaterThan);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 25 }));
        Assert.False(compiled(new TestEntity { Age = 18 }));
    }

    [Fact]
    public void ToConditionalExpression_IntCompareOperation_GreaterThanOrEqual()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, 18, CompareOperation.GreaterThanOrEqual);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 18 }));
        Assert.True(compiled(new TestEntity { Age = 25 }));
        Assert.False(compiled(new TestEntity { Age = 17 }));
    }

    [Fact]
    public void ToConditionalExpression_IntCompareOperation_LessThan()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, 18, CompareOperation.LessThan);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 10 }));
        Assert.False(compiled(new TestEntity { Age = 18 }));
    }

    [Fact]
    public void ToConditionalExpression_IntCompareOperation_LessThanOrEqual()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, 18, CompareOperation.LessThanOrEqual);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 18 }));
        Assert.True(compiled(new TestEntity { Age = 10 }));
        Assert.False(compiled(new TestEntity { Age = 25 }));
    }

    [Fact]
    public void ToConditionalExpression_IntCompareOperation_Equal()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, 25, CompareOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 25 }));
        Assert.False(compiled(new TestEntity { Age = 30 }));
    }

    [Fact]
    public void ToConditionalExpression_IntCompareOperation_NotEqual()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, 25, CompareOperation.NotEqual);

        var compiled = expr.Compile();
        Assert.False(compiled(new TestEntity { Age = 25 }));
        Assert.True(compiled(new TestEntity { Age = 30 }));
    }

    // -------------------------------------------------------------------------
    // Bool EqualOperation overload
    // -------------------------------------------------------------------------

    [Fact]
    public void ToConditionalExpression_BoolEqualOperation_Equal_True()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.IsActive, true, EqualOperation.Equal);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { IsActive = true }));
        Assert.False(compiled(new TestEntity { IsActive = false }));
    }

    [Fact]
    public void ToConditionalExpression_BoolEqualOperation_NotEqual_True()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.IsActive, true, EqualOperation.NotEqual);

        var compiled = expr.Compile();
        Assert.False(compiled(new TestEntity { IsActive = true }));
        Assert.True(compiled(new TestEntity { IsActive = false }));
    }

    // -------------------------------------------------------------------------
    // Class (reference) type overload — no operation, plain value with query string
    // -------------------------------------------------------------------------

    [Fact]
    public void ToConditionalExpression_StringClass_QueryString_Like()
    {
        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Email, "like:@gmail");

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Email = "user@gmail.com" }));
        Assert.False(compiled(new TestEntity { Email = "user@yahoo.com" }));
    }

    // -------------------------------------------------------------------------
    // With explicit default expression fallback
    // -------------------------------------------------------------------------

    [Fact]
    public void ToConditionalExpression_WithDefaultExpression_UsedWhenNoQueryString()
    {
        var defaultExpr = (System.Linq.Expressions.Expression<Func<TestEntity, bool>>)(e => e.Age == 25);

        var expr = Specification<TestEntity>.ToConditionalExpression(
            e => e.Age, 25, defaultExpr);

        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 25 }));
        Assert.False(compiled(new TestEntity { Age = 30 }));
    }
}

