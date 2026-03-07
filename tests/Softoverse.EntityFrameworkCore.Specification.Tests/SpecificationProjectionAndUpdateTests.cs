using System.Linq.Expressions;

using Softoverse.EntityFrameworkCore.Specification.Implementation;
using Softoverse.EntityFrameworkCore.Specification.Tests.Models;

namespace Softoverse.EntityFrameworkCore.Specification.Tests;

/// <summary>
/// Tests for SetProjection and SetExecuteUpdateExpression / AddExecuteUpdateProperties.
/// </summary>
public class SpecificationProjectionAndUpdateTests
{
    [Fact]
    public void SetProjection_SetsProjectionExpression()
    {
        var spec = new Specification<TestEntity>();
        Expression<Func<TestEntity, object>> proj = e => new TestEntity { Id = e.Id, Name = e.Name };

        spec.SetProjection(proj);

        Assert.NotNull(spec.ProjectionExpression);
    }

    [Fact]
    public void SetProjection_ReplacesExistingProjection()
    {
        var spec = new Specification<TestEntity>();
        Expression<Func<TestEntity, object>> proj1 = e => e.Name;
        Expression<Func<TestEntity, object>> proj2 = e => e.Email;
        spec.SetProjection(proj1);
        spec.SetProjection(proj2);

        Assert.NotNull(spec.ProjectionExpression);
    }

    [Fact]
    public void ProjectionExpression_DefaultIsNull()
    {
        var spec = new Specification<TestEntity>();

        Assert.Null(spec.ProjectionExpression);
    }

    [Fact]
    public void SetExecuteUpdateExpression_SetsAction()
    {
        var spec = new Specification<TestEntity>();

        spec.SetExecuteUpdateExpression(setters =>
            setters.SetProperty(e => e.IsActive, true));

        Assert.NotNull(spec.ExecuteUpdateExpression);
    }

    [Fact]
    public void SetExecuteUpdateExpression_ReplacesExistingAction()
    {
        var spec = new Specification<TestEntity>();
        spec.SetExecuteUpdateExpression(setters => setters.SetProperty(e => e.IsActive, true));
        spec.SetExecuteUpdateExpression(setters => setters.SetProperty(e => e.Name, "Updated"));

        Assert.NotNull(spec.ExecuteUpdateExpression);
    }

    [Fact]
    public void ExecuteUpdateExpression_DefaultIsNull()
    {
        var spec = new Specification<TestEntity>();

        Assert.Null(spec.ExecuteUpdateExpression);
    }

    [Fact]
    public void AddExecuteUpdateProperties_AddsPropertySelector()
    {
        var spec = new Specification<TestEntity>();

        spec.AddExecuteUpdateProperties(e => e.Name);

        Assert.Single(spec.ExecuteUpdateProperties);
    }

    [Fact]
    public void AddExecuteUpdateProperties_MultipleProperties_AllAdded()
    {
        var spec = new Specification<TestEntity>();

        spec.AddExecuteUpdateProperties(e => e.Name);
        spec.AddExecuteUpdateProperties(e => e.Email);
        spec.AddExecuteUpdateProperties(e => e.IsActive);

        Assert.Equal(3, spec.ExecuteUpdateProperties.Count);
    }

    [Fact]
    public void ExecuteUpdateProperties_DefaultIsEmpty()
    {
        var spec = new Specification<TestEntity>();

        Assert.Empty(spec.ExecuteUpdateProperties);
    }
}






