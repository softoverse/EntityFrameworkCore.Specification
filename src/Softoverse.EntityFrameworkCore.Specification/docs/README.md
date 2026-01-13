# Specification Pattern for Entity Framework Core

[![NuGet](https://img.shields.io/nuget/v/Softoverse.EntityFrameworkCore.Specification.svg)](https://www.nuget.org/packages/Softoverse.EntityFrameworkCore.Specification)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A powerful, lightweight implementation of the Specification Pattern for Entity Framework Core. Encapsulate your query logic into reusable, testable classes and keep your repositories clean.

## Table of Contents
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Features](#core-features)
    - [Filtering (Criteria)](#filtering-criteria)
    - [Includes (Eager Loading)](#includes-eager-loading)
    - [Ordering](#ordering)
    - [Projections](#projections)
    - [No-Tracking & Split Queries](#no-tracking--split-queries)
- [Advanced Usage](#advanced-usage)
    - [Dynamic Expression Generation](#dynamic-expression-generation)
    - [Smart Filtering (ToConditionalExpressionInternal)](#smart-filtering-toconditionalexpressioninternal)
    - [Bulk Updates (ExecuteUpdate)](#bulk-updates-executeupdate)
    - [Expression Combiner](#expression-combiner)
- [Repository Integration](#repository-integration)
- [CQRS Integration](#cqrs-integration)
- [Full API Reference](#full-api-reference)
    - [Interfaces](#interfaces)
    - [Classes](#classes)
    - [Enums](#enums)
    - [Extension Methods](#extension-methods)
- [Best Practices](#best-practices)
- [Compatibility](#compatibility)
- [License](#license)

---

## Installation

Install via NuGet Package Manager Console:
```powershell
Install-Package Softoverse.EntityFrameworkCore.Specification
```

Or via .NET CLI:
```sh
dotnet add package Softoverse.EntityFrameworkCore.Specification
```

---

## Quick Start

### 1. Define a Specification
```csharp
public class ActivePremiumUsersSpec : Specification<User>
{
    public ActivePremiumUsersSpec()
    {
        Criteria = u => u.IsActive && u.IsPremium;
        
        // Eager load related data
        Include(u => u.Profile);
        Include(u => u.Orders)
            .ThenInclude(o => o.OrderItems);
            
        // Ordering
        AddOrderByDescending(u => u.CreatedAt);
        
        // Query options
        AsNoTracking = true;
    }
}
```

### 2. Apply it in your Repository
```csharp
public async Task<List<User>> GetPremiumUsersAsync(ISpecification<User> spec)
{
    // ApplySpecification is an extension method for IQueryable<T>, DbSet<T>, and DbContext
    return await _context.Users.ApplySpecification(spec).ToListAsync();
}
```

---

## Core Features

### Filtering (Criteria)
The `Criteria` property defines the `Where` clause of your query.

```csharp
var spec = new Specification<User>
{
    Criteria = u => u.Age > 18
};
```

You can also pass multiple expressions to the constructor:
```csharp
var spec = new Specification<User>(
    [u => u.IsActive, u => u.Email.Contains("@gmail.com")], 
    CombineType.And
);
```

### Includes (Eager Loading)
The library provides a fluent API for `Include` and `ThenInclude`, supporting both collections and single navigation properties.

#### Simple & Nested Includes
```csharp
spec.Include(u => u.Orders)
    .ThenInclude(o => o.Items);
```

#### String-based Includes
```csharp
spec.IncludeString("Orders.Items");
```

#### Filtered Includes (EF Core 5+)
You can apply filters directly within the Include:
```csharp
spec.Include(u => u.Orders.Where(o => o.Status == OrderStatus.Shipped));
```

### Ordering
Easily add sort logic to your specifications.
```csharp
spec.AddOrderBy(u => u.LastName);
spec.AddOrderByDescending(u => u.CreatedAt);
```

### Projections
Use `SetProjection` to select only the fields you need.
```csharp
spec.SetProjection(u => new { u.Id, u.FullName });
```

### No-Tracking & Split Queries
Optimize performance for read-only or complex queries.
```csharp
spec.AsNoTracking = true;
spec.AsSplitQuery = true;
```

---

## Advanced Usage

### Dynamic Expression Generation
The `ToConditionalExpression` static methods allow you to generate expressions dynamically based on values and operators.

```csharp
// Generates: u => u.Age > 25
var ageExp = Specification<User>.ToConditionalExpression(
    u => u.Age, 
    25, 
    CompareOperation.GreaterThan
);

// Generates: u => u.Name == "John"
var nameExp = Specification<User>.ToConditionalExpression(
    u => u.Name, 
    "John", 
    EqualOperation.Equal
);
```

### Smart Filtering (ToConditionalExpressionInternal)
The `ToConditionalExpression` method internally uses `ToConditionalExpressionInternal` to provide "Smart Filtering" capabilities. This allows you to generate complex expressions using simple string patterns, which is especially useful for handling query string parameters from an API.

#### Supported Operators
| Operator | Description | Example |
| :--- | :--- | :--- |
| `eq` / `ne` | Equal / Not Equal | `eq:John`, `ne:John` |
| `gt` / `gte` | Greater Than / Or Equal | `gt:18`, `gte:18` |
| `lt` / `lte` | Less Than / Or Equal | `lt:18`, `lte:18` |
| `eqci` | Equal (Case-Insensitive) | `eqci:john` |
| `like` / `likeci`| Contains / Case-Insensitive | `like:jo`, `likeci:JO` |
| `range` | Range (Inclusive) | `range:18,30` |
| `in` / `nin` | In List / Not In List | `in:1,2,3`, `nin:1,2,3` |
| `inci` / `ninci`| In/Not In List (Case-Insensitive) | `inci:a,b`, `ninci:c,d` |
| `inlike` / `ninlike` | In/Not In List (Pattern Match) | `inlike:a,b`, `ninlike:x,y` |
| `inlikeci` / `ninlikeci` | Case-Insensitive Pattern Match | `inlikeci:A,B` |

#### Usage Example
```csharp
// These strings would typically come from your API query parameters
string ageFilter = "gt:18";
string nameFilter = "like:John";
string categoryFilter = "in:Electronics,Books";

var spec = new Specification<User>();
spec.Criteria = Specification<User>.ToConditionalExpression(u => u.Age, ageFilter);
spec.Criteria = spec.Criteria.And(Specification<User>.ToConditionalExpression(u => u.Name, nameFilter));
spec.Criteria = spec.Criteria.And(Specification<User>.ToConditionalExpression(u => u.Category, categoryFilter));
```

### Bulk Updates (ExecuteUpdate)
Integration with EF Core's `ExecuteUpdate` for efficient bulk operations.

```csharp
// Option 1: Using property selectors
var spec = new Specification<User>
{
    Criteria = u => u.Id == 1,
    ExecuteUpdateProperties = [u => u.Name, u => u.Email]
};
// After updating properties on the user object:
await context.Users.ExecuteUpdateAsync(spec, userObject);

// Option 2: Using fluent SetProperty
spec.SetExecuteUpdateExpression(setters => setters
    .SetProperty(u => u.IsActive, true)
    .SetProperty(u => u.LastLogin, DateTime.UtcNow));
```

### Expression Combiner
The `ExpressionBuilder` provides extension methods to combine expressions manually.
```csharp
Expression<Func<User, bool>> criteria1 = u => u.IsActive;
Expression<Func<User, bool>> criteria2 = u => u.Age > 18;

var combined = criteria1.And(criteria2);
var either = criteria1.Or(criteria2);
```

---

## Repository Integration

A typical generic repository implementation:

```csharp
public class Repository<T> where T : class
{
    private readonly DbContext _context;
    public Repository(DbContext context) => _context = context;

    public async Task<List<T>> ListAsync(ISpecification<T> spec)
    {
        return await _context.Set<T>()
            .ApplySpecification(spec)
            .ToListAsync();
    }
    
    public async Task<T?> GetAsync(ISpecification<T> spec)
    {
        return await _context.Set<T>()
            .ApplySpecification(spec)
            .FirstOrDefaultAsync();
    }
}
```

---

## CQRS Integration

For projects using the CQRS pattern (e.g., with MediatR), you can use the `ISpecificationRequest<TEntity>` interface to bridge your queries and specifications.

### Defining a Specification Request
Inherit your query classes from `ISpecificationRequest<TEntity>` to standardize how specifications are retrieved from requests.

```csharp
public class GetActiveUsersQuery : ISpecificationRequest<User>
{
    public string SearchTerm { get; set; }

    public ISpecification<User> GetSpecification(bool asNoTracking = false, bool asSplitQuery = false)
    {
        var spec = new Specification<User>
        {
            AsNoTracking = asNoTracking,
            AsSplitQuery = asSplitQuery
        };

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            spec.Criteria = u => u.IsActive && u.Name.Contains(SearchTerm);
        }
        else
        {
            spec.Criteria = u => u.IsActive;
        }

        spec.Include(u => u.Profile);
        spec.AddOrderBy(u => u.Name);

        return spec;
    }
}
```

### Usage in a Query Handler
In your handler, simply call `GetSpecification()` to obtain the specification and apply it to your `IQueryable`.

```csharp
public class GetActiveUsersHandler
{
    private readonly ApplicationDbContext _context;

    public GetActiveUsersHandler(ApplicationDbContext context) => _context = context;

    public async Task<List<User>> Handle(GetActiveUsersQuery request)
    {
        var spec = request.GetSpecification(asNoTracking: true);
        
        return await _context.Users
            .ApplySpecification(spec)
            .ToListAsync();
    }
}
```

---

## Full API Reference

### Interfaces

#### `ISpecification<TEntity>`
The primary interface for defining specifications.
- `Criteria`: The filter expression.
- `AsNoTracking`: Boolean for tracking behavior.
- `AsSplitQuery`: Boolean for query splitting behavior.
- `OrderByExpression`: Primary sort expression.
- `OrderByDescendingExpression`: Primary descending sort expression.
- `ProjectionExpression`: Expression for projecting to a different type.
- `ExecuteUpdateExpression`: Action for bulk updates using `UpdateSettersBuilder`.
- `ExecuteUpdateProperties`: List of property selectors for object-based bulk updates.

#### `ISpecificationRequest<TEntity>`
Used for bridging Query objects (CQRS) with Specifications.
- `GetSpecification(bool asNoTracking, bool asSplitQuery)`: Returns an `ISpecification<TEntity>`.

#### `ISpecificationForPrimaryKey`
Base interface for specifications that might use a primary key.
- `PrimaryKey`: The primary key value.

#### `IIncludableSpecification<TEntity, TProperty>`
Enables fluent `ThenInclude` chaining.
- `ThenInclude<TNextProperty>(expression)`: Chains a sub-property load.

### Classes

#### `Specification<TEntity>`
The base implementation of `ISpecification<TEntity>`.
- **Constructors**:
    - `Specification()`: Empty specification.
    - `Specification(List<Expression>, CombineType)`: Combines multiple expressions.
    - `Specification(object primaryKey)`: Specification for a specific record.
- **Methods**:
    - `Include(expression)`: Adds an eager load.
    - `IncludeString(path)`: Adds a string-based eager load.
    - `AddOrderBy(expression)`: Sets the ascending order.
    - `AddOrderByDescending(expression)`: Sets the descending order.
    - `SetProjection(expression)`: Sets the selection projection.
    - `SetExecuteUpdateExpression(action)`: Sets the fluent bulk update logic.
    - `AddExecuteUpdateProperties(expression)`: Adds a property for bulk update.
    - `ToConditionalExpression(...)`: Static helper to generate expressions from values/operators.

#### `ExpressionGenerator<TEntity>` (Static)
Helper for generating complex expressions dynamically.
- `BuildUpdateExpression(properties, model)`: Generates an `ExecuteUpdate` expression from an object and selected properties.
- `BuildUpdateExpression(propertyUpdates)`: Generates an `ExecuteUpdate` expression from a dictionary of property paths and values.

### Enums

#### `CombineType`
Used when combining multiple expressions in the constructor.
- `And`: All criteria must be met.
- `Or`: At least one criteria must be met.

#### `EqualOperation` / `CompareOperation`
Used in `ToConditionalExpression` to define comparison logic.
- `Equal`, `NotEqual`, `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`.

### Extension Methods

#### `SpecificationEvaluator`
- `ApplySpecification(queryable, specification)`: Applies all specification logic to an `IQueryable`.
- `ApplySpecification(dbSet, specification)`: Helper for `DbSet`.
- `ApplySpecification(dbContext, specification)`: Helper for `DbContext`.

#### `ExpressionBuilder`
- `And(left, right)`: Combines two expressions with AND.
- `Or(left, right)`: Combines two expressions with OR.
- `Not(expression)`: Negates an expression.
- `When(expression, condition)`: Conditionally applies an expression.
- `CombineWithAnd(expressions)`: Aggregates multiple expressions with AND.
- `CombineWithOr(expressions)`: Aggregates multiple expressions with OR.

---

## Best Practices
1.  **Encapsulation**: Put complex query logic inside named Specification classes instead of building them in your services or repositories.
2.  **Reusability**: Use small, focused specifications and combine them if necessary.
3.  **Read-Only**: Always use `AsNoTracking = true` for specifications intended for read-only display to improve performance.

---

## Compatibility
- .NET 6.0+
- Entity Framework Core 6.0+

---

## License
Licensed under the Apache License, Version 2.0. See [LICENSE](LICENSE) for details.

## Support
For issues, please open a GitHub issue in the repository.
