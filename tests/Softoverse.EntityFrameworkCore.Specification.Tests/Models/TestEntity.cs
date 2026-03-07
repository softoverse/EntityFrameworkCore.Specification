namespace Softoverse.EntityFrameworkCore.Specification.Tests.Models;

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public TestProfile? Profile { get; set; }
    public List<TestOrder> Orders { get; set; } = [];
}

public class TestProfile
{
    public int Id { get; set; }
    public string Bio { get; set; } = string.Empty;
    public int TestEntityId { get; set; }
    public TestEntity? TestEntity { get; set; }
}

public class TestOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TestEntityId { get; set; }
    public TestEntity? TestEntity { get; set; }
    public List<TestOrderItem> Items { get; set; } = [];
}

public class TestOrderItem
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int TestOrderId { get; set; }
    public TestOrder? TestOrder { get; set; }
}

