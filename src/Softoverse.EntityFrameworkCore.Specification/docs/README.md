# Specification Pattern for Entity Framework Core - NuGet Package Documentation

## Overview
The **Specification Pattern for Entity Framework Core** NuGet package provides a structured approach to implementing the Specification pattern in any .NET project using Entity Framework Core. This package allows developers to encapsulate query logic into reusable specifications, improving maintainability and readability.

## Installation
To install the package, use one of the following methods:

### Package Manager Console
```powershell
Install-Package Softoverse.EntityFrameworkCore.Specification
```

### .NET CLI
```sh
dotnet add package Softoverse.EntityFrameworkCore.Specification
```

### PackageReference
If you prefer to add the dependency manually, update your `.csproj` file:
```xml
<ItemGroup>
    <PackageReference Include="Softoverse.EntityFrameworkCore.Specification" Version="2.1.0" />
</ItemGroup>
```

## Proper documentation is coming soon...

<!-- ## Usage
### 1. Define a Specification
Create a specification class by inheriting from `Specification<T>`.

```csharp
using YourPackageNamespace;
using System;
using System.Linq.Expressions;

public class ActiveUsersSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.IsActive;
    }
}
```

### 2. Apply Specification in Repository
Use the `ApplySpecification` method to filter queries using the defined specification.

```csharp
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserRepository
{
    private readonly ApplicationDbContext _context;
    
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<User>> GetActiveUsersAsync()
    {
        var spec = new ActiveUsersSpecification();
        return await _context.Users.ApplySpecification(spec).ToListAsync();
    }
}
```

## Extension Method
Ensure that your `DbSet<T>` supports applying specifications by adding an extension method:

```csharp
using Microsoft.EntityFrameworkCore;
using System.Linq;

public static class SpecificationExtensions
{
    public static IQueryable<T> ApplySpecification<T>(this IQueryable<T> query, Specification<T> specification) where T : class
    {
        return query.Where(specification.ToExpression());
    }
}
```

## Benefits of Using the Specification Pattern
- **Encapsulates Query Logic**: Centralizes filtering logic in reusable specifications.
- **Enhances Code Readability**: Separates query logic from repository logic.
- **Encourages Reusability**: Specifications can be reused across different repositories.

## Compatibility
- .NET 6.0+
- Entity Framework Core 6+

## License
This package is released under the Apache 2.0 License.

## Contributing
Contributions are welcome! Please submit a pull request or open an issue if you have suggestions or improvements.

## Support
For any issues, please open a GitHub issue in the repository or contact the package maintainer.
 -->
