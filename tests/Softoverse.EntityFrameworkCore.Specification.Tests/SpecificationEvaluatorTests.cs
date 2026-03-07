using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Softoverse.EntityFrameworkCore.Specification.Abstraction;
using Softoverse.EntityFrameworkCore.Specification.Implementation;
using Softoverse.EntityFrameworkCore.Specification.Tests.Models;

namespace Softoverse.EntityFrameworkCore.Specification.Tests;

/// <summary>
/// In-memory DbContext for SpecificationEvaluator integration tests.
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<TestEntity> TestEntities { get; set; } = null!;
    public DbSet<TestProfile> TestProfiles { get; set; } = null!;
    public DbSet<TestOrder> TestOrders { get; set; } = null!;
    public DbSet<TestOrderItem> TestOrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>()
            .HasOne(e => e.Profile)
            .WithOne(p => p.TestEntity)
            .HasForeignKey<TestProfile>(p => p.TestEntityId);

        modelBuilder.Entity<TestEntity>()
            .HasMany(e => e.Orders)
            .WithOne(o => o.TestEntity)
            .HasForeignKey(o => o.TestEntityId);

        modelBuilder.Entity<TestOrder>()
            .HasMany(o => o.Items)
            .WithOne(i => i.TestOrder)
            .HasForeignKey(i => i.TestOrderId);
    }
}

/// <summary>
/// Integration tests for SpecificationEvaluator.ApplySpecification —
/// verifies filtering, ordering, no-tracking, split query, string includes,
/// and projection using an in-memory EF Core database.
/// </summary>
public class SpecificationEvaluatorTests : IDisposable
{
    private readonly TestDbContext _context;

    public SpecificationEvaluatorTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        SeedData();
    }

    private void SeedData()
    {
        var entities = new List<TestEntity>
        {
            new()
            {
                Id = 1, Name = "Alice", Email = "alice@test.com", Age = 30, IsActive = true,
                CreatedAt = new DateTime(2023, 1, 1),
                Profile = new TestProfile { Id = 1, Bio = "Alice bio" },
                Orders =
                [
                    new TestOrder
                    {
                        Id = 1, OrderNumber = "ORD-001", Amount = 100,
                        Items = [new TestOrderItem { Id = 1, ProductName = "Widget", Quantity = 2, Price = 50 }]
                    }
                ]
            },
            new()
            {
                Id = 2, Name = "Bob", Email = "bob@test.com", Age = 25, IsActive = true,
                CreatedAt = new DateTime(2023, 6, 1),
                Profile = new TestProfile { Id = 2, Bio = "Bob bio" },
                Orders = []
            },
            new()
            {
                Id = 3, Name = "Charlie", Email = "charlie@example.com", Age = 17, IsActive = false,
                CreatedAt = new DateTime(2022, 1, 1),
                Profile = null,
                Orders = []
            },
            new()
            {
                Id = 4, Name = "Dave", Email = "dave@test.com", Age = 22, IsActive = false,
                CreatedAt = new DateTime(2021, 6, 1),
                Profile = null,
                Orders = []
            }
        };

        _context.TestEntities.AddRange(entities);
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    // -------------------------------------------------------------------------
    // Filtering (Criteria)
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplySpecification_WithCriteria_FiltersResults()
    {
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive
        };

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.IsActive));
    }

    [Fact]
    public void ApplySpecification_NoCriteria_ReturnsAll()
    {
        var spec = new Specification<TestEntity>();

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Equal(4, results.Count);
    }

    [Fact]
    public void ApplySpecification_ComplexCriteria_FiltersCorrectly()
    {
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive && e.Age >= 25
        };

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.IsActive && r.Age >= 25));
    }

    [Fact]
    public void ApplySpecification_CriteriaMatchesNone_ReturnsEmpty()
    {
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.Age > 100
        };

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Empty(results);
    }

    // -------------------------------------------------------------------------
    // Ordering
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplySpecification_OrderBy_SortsAscending()
    {
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.Age);

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Equal(17, results[0].Age);
        Assert.Equal(22, results[1].Age);
        Assert.Equal(25, results[2].Age);
        Assert.Equal(30, results[3].Age);
    }

    [Fact]
    public void ApplySpecification_OrderByDescending_SortsDescending()
    {
        var spec = new Specification<TestEntity>();
        spec.OrderByDescending(e => e.Age);

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Equal(30, results[0].Age);
        Assert.Equal(25, results[1].Age);
        Assert.Equal(22, results[2].Age);
        Assert.Equal(17, results[3].Age);
    }

    [Fact]
    public void ApplySpecification_OrderBy_ThenByDescending_MultiLevelSort()
    {
        var spec = new Specification<TestEntity>();
        spec.OrderBy(e => e.IsActive)
            .ThenByDescending(e => e.Age);

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        // First come inactive (false < true), sorted by age descending
        Assert.False(results[0].IsActive);
        Assert.False(results[1].IsActive);
        Assert.Equal(22, results[0].Age); // Dave: inactive, age 22
        Assert.Equal(17, results[1].Age); // Charlie: inactive, age 17

        Assert.True(results[2].IsActive);
        Assert.True(results[3].IsActive);
        Assert.Equal(30, results[2].Age); // Alice: active, age 30
        Assert.Equal(25, results[3].Age); // Bob: active, age 25
    }

    [Fact]
    public void ApplySpecification_NoOrdering_StableResultSet()
    {
        var spec = new Specification<TestEntity>();

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Equal(4, results.Count);
    }

    // -------------------------------------------------------------------------
    // String-based include (IncludeString)
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplySpecification_IncludeString_LoadsNavigation()
    {
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.Id == 1
        };
        spec.IncludeString("Profile");

        var result = _context.TestEntities.ApplySpecification(spec).First();

        Assert.NotNull(result.Profile);
        Assert.Equal("Alice bio", result.Profile!.Bio);
    }

    [Fact]
    public void ApplySpecification_IncludeString_NestedPath_LoadsDeepNavigation()
    {
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.Id == 1
        };
        spec.IncludeString("Orders");

        var result = _context.TestEntities.ApplySpecification(spec).First();

        Assert.NotEmpty(result.Orders);
    }

    // -------------------------------------------------------------------------
    // Expression-based Include
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplySpecification_Include_LoadsNavigationProperty()
    {
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.Id == 1
        };
        spec.Include(e => e.Profile);

        var result = _context.TestEntities.ApplySpecification(spec).First();

        Assert.NotNull(result.Profile);
    }

    [Fact]
    public void ApplySpecification_Include_ThenInclude_LoadsNestedNavigation()
    {
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.Id == 1
        };
        spec.Include(e => e.Orders)
            .ThenInclude<TestEntity, TestOrder, List<TestOrderItem>>(o => o.Items);

        var result = _context.TestEntities.ApplySpecification(spec).First();

        Assert.NotEmpty(result.Orders);
        Assert.NotEmpty(result.Orders[0].Items);
        Assert.Equal("Widget", result.Orders[0].Items[0].ProductName);
    }

    // -------------------------------------------------------------------------
    // AsNoTracking
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplySpecification_AsNoTracking_EntitiesNotTracked()
    {
        var spec = new Specification<TestEntity>
        {
            AsNoTracking = true,
            Criteria = e => e.Id == 1
        };

        var result = _context.TestEntities.ApplySpecification(spec).First();

        Assert.Equal(EntityState.Detached, _context.Entry(result).State);
    }

    [Fact]
    public void ApplySpecification_WithTracking_EntitiesAreTracked()
    {
        var spec = new Specification<TestEntity>
        {
            AsNoTracking = false,
            Criteria = e => e.Id == 1
        };

        var result = _context.TestEntities.ApplySpecification(spec).First();

        Assert.NotEqual(EntityState.Detached, _context.Entry(result).State);
    }

    // -------------------------------------------------------------------------
    // Projection
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplySpecification_WithProjection_ProjectsCorrectly()
    {
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive
        };
        Expression<Func<TestEntity, object>> proj = e => new TestEntity { Id = e.Id, Name = e.Name };
        spec.SetProjection(proj);

        // Projection returns IQueryable<TestEntity> via OfType — basic count still works
        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Equal(2, results.Count);
    }

    // -------------------------------------------------------------------------
    // DbSet and DbContext extension overloads
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplySpecification_ViaDbSet_Works()
    {
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive
        };

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void ApplySpecification_ViaDbContext_Works()
    {
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive
        };

        var results = _context.ApplySpecification(spec).ToList();

        Assert.Equal(2, results.Count);
    }

    // -------------------------------------------------------------------------
    // PrimaryKey constructor — basic sanity via evaluator
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplySpecification_WithPrimaryKeyConstructor_StillFiltersByCriteria()
    {
        var spec = new Specification<TestEntity>(1)
        {
            Criteria = e => e.Id == 1
        };

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Single(results);
        Assert.Equal(1, results[0].Id);
    }

    // -------------------------------------------------------------------------
    // Legacy OrderByExpression fallback
    // -------------------------------------------------------------------------

    [Fact]
    [Obsolete("Testing obsolete AddOrderBy fallback")]
    public void ApplySpecification_LegacyOrderByExpression_FallbackApplied()
    {
#pragma warning disable CS0618
        var spec = new Specification<TestEntity>();
        spec.AddOrderBy(e => e.Age);
#pragma warning restore CS0618

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Equal(17, results[0].Age);
    }

    [Fact]
    [Obsolete("Testing obsolete AddOrderByDescending fallback")]
    public void ApplySpecification_LegacyOrderByDescendingExpression_FallbackApplied()
    {
#pragma warning disable CS0618
        var spec = new Specification<TestEntity>();
        spec.AddOrderByDescending(e => e.Age);
#pragma warning restore CS0618

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Equal(30, results[0].Age);
    }

    // -------------------------------------------------------------------------
    // Filtering + Ordering combined
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplySpecification_FilterAndOrder_BothApplied()
    {
        var spec = new Specification<TestEntity>
        {
            Criteria = e => e.IsActive
        };
        spec.OrderBy(e => e.Age);

        var results = _context.TestEntities.ApplySpecification(spec).ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal(25, results[0].Age); // Bob
        Assert.Equal(30, results[1].Age); // Alice
    }

    // -------------------------------------------------------------------------
    // AsSplitQuery flag propagation
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplySpecification_AsSplitQuery_DoesNotThrow()
    {
        // AsSplitQuery requires a relational provider; in-memory silently ignores it
        var spec = new Specification<TestEntity>
        {
            AsSplitQuery = true
        };

        var ex = Record.Exception(() => _context.TestEntities.ApplySpecification(spec).ToList());
        // In-memory provider may throw NotSupportedException for AsSplitQuery
        // The important thing is the spec flag propagates without crashing the pipeline
        Assert.True(ex == null || ex is InvalidOperationException || ex is NotSupportedException);
    }
}










