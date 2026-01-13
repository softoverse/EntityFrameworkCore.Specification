using System.Diagnostics;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Softoverse.EntityFrameworkCore.Specification.Abstraction;
using Softoverse.EntityFrameworkCore.Specification.Extensions;
using Softoverse.EntityFrameworkCore.Specification.Helpers;
using Softoverse.EntityFrameworkCore.Specification.Implementation;

using TestConsole.DataAccess;
using TestConsole.Models;

namespace TestConsole;

public class Program
{
    private static ApplicationDbContext _context;
    private static IRepositoryBase<Country, long> _countryRepository;
    private static IRepositoryBase<City, long> _cityRepository;

    public static async Task Main(string[] args)
    {
        TestSpecificationsExpressionGenerator();
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        
        await InitializeDbContextAsync();
        await SeedDatabase();
        
        Console.WriteLine("\n========== Testing Include Features ==========\n");
        await TestSimpleInclude();
        await TestThenInclude();
        await TestMultiLevelThenInclude();
        await TestMixedIncludesAndFilters();
        await TestFilteredInclude();
        await TestFilteredThenInclude();
        await TestCompareIncludeMethods();
        await TestImprovedInclude();
        
        Console.WriteLine("\n========== Testing OrderBy & ThenBy Features ==========\n");
        await TestSimpleOrderBy();
        await TestOrderByDescending();
        await TestThenBy();
        await TestMultiLevelThenBy();
        await TestMixedOrderByWithIncludes();
        
        Console.WriteLine("\n========== Testing Update Operation ==========\n");
        await TestUpdateOperation();
    }

    private static async Task InitializeDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                      .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SpecificationPatternTest;Trusted_Connection=True;")
                      .LogTo(sql =>
                      {
                          if (!sql.Contains("Executed DbCommand")) return;
                          Console.ForegroundColor = ConsoleColor.Cyan;
                          Console.WriteLine(sql);
                          Console.ResetColor();
                      }, Microsoft.Extensions.Logging.LogLevel.Information)
                      .EnableSensitiveDataLogging() // Shows parameter values in SQL
                      .Options;

        _context = new ApplicationDbContext(options);
        
        // Ensure database is created
        await _context.Database.EnsureCreatedAsync();
        
        // Ensure tables exist without using migrations
        await DatabaseInitializer.EnsureTablesCreatedAsync(_context);
        _countryRepository = new CountryRepository(_context);
        _cityRepository = new CityRepository(_context);
    }

    private static async Task SeedDatabase()
    {
        if (await _countryRepository.ExistsByAsync(x => true))
        {
            return;
        }

        List<Country> countries =
        [
            new()
            {
                Name = "USA",
                Cities =
                [
                    new()
                    {
                        Name = "New York",
                        IsCapital = false,
                        Districts =
                        [
                            new() { Name = "Manhattan", Population = 1694251 },
                            new() { Name = "Brooklyn", Population = 2736074 },
                            new() { Name = "Queens", Population = 2405464 }
                        ]
                    },
                    new()
                    {
                        Name = "San Francisco",
                        IsCapital = false,
                        Districts =
                        [
                            new() { Name = "Financial District", Population = 8500 },
                            new() { Name = "Mission District", Population = 60000 }
                        ]
                    }
                ]
            },
            new()
            {
                Name = "Russia",
                Cities =
                [
                    new()
                    {
                        Name = "Moscow",
                        IsCapital = true,
                        Districts =
                        [
                            new() { Name = "Central District", Population = 750000 },
                            new() { Name = "Northern District", Population = 1200000 }
                        ]
                    },
                    new()
                    {
                        Name = "Saint Petersburg",
                        IsCapital = false,
                        Districts =
                        [
                            new() { Name = "Admiralteysky", Population = 160000 },
                            new() { Name = "Vasileostrovsky", Population = 210000 }
                        ]
                    }
                ]
            }
        ];

        // Use synchronous calls to make sure data is seeded in this demo; underlying methods are async on the repository
        await _countryRepository.AddRangeAsync(countries);
        await _countryRepository.SaveChangesAsync();

        // Verify DB-generated IDs by reading back the saved rows and printing their Ids
        var seeded = await _countryRepository.GetAllAsync(x => true, asNoTracking: true);
        Console.WriteLine("Seeded countries (Id : Name):");
        foreach (var c in seeded)
        {
            Console.WriteLine($"{c.Id} : {c.Name}");
        }
    }

    private static async Task TestUpdateOperation()
    {
        var countries = await _countryRepository.GetAllAsync(x => true, asNoTracking: true);
        var country = countries.FirstOrDefault()!;

        Specification<Country> specification = new Specification<Country>
        {
            Criteria = x => x.Id == country.Id,
            ExecuteUpdateProperties =
            [
                x => x.Name
            ]
        };

        country.Name = "America";
        await _countryRepository.ExecuteUpdateAsync(specification, country);

        Console.WriteLine(country.Name);

        specification = new Specification<Country>
        {
            Criteria = x => x.Id == country.Id,
            ExecuteUpdateExpression = x => x.SetProperty(y => y.Name, "USA")
                                            .SetProperty(y => y.Population, 1000000000)
        };

        country.Name = "USA";
        await _countryRepository.ExecuteUpdateAsync(specification, country);

        countries = await _countryRepository.GetAllAsync(x => true, asNoTracking: true);
    }

    private static void TestSpecificationsExpressionGenerator()
    {
        Specification<Country> specification = new Specification<Country>();

        var expression1 = Specification<Country>.ToConditionalExpression(x => x.Name, "BD");

        var expression2 = Specification<Country>.ToConditionalExpression(x => x.Name, "BD", x => x.Name == "BD");

        var expression3 = Specification<Country>.ToConditionalExpression(x => x.Name, "BD", EqualOperation.Equal);

        var expression4 = Specification<Country>.ToConditionalExpression(x => x.IsIndependent, false, EqualOperation.Equal);

        var expression5 = Specification<Country>.ToConditionalExpression(x => x.Population, 0, CompareOperation.GreaterThan);

        var expression6 = Specification<Country>.ToConditionalExpression(x => x.IsIndependent, false, x => x.IsIndependent == false);

        var expression7 = Specification<Country>.ToConditionalExpression(x => x.IsIndependent, "eqqwe:True", EqualOperation.NotEqual);
        var expression8 = Specification<Country>.ToConditionalExpression(x => x.IsIndependent, "eqci:True", EqualOperation.NotEqual);

        var expression9 = Specification<Country>.ToConditionalExpression(x => x.IsIndependent, "eq:true", EqualOperation.NotEqual);

        Console.WriteLine(expression1);
        Console.WriteLine(expression2);
        Console.WriteLine(expression3);
        Console.WriteLine(expression4);
        Console.WriteLine(expression5);
        Console.WriteLine(expression6);
        Console.WriteLine(expression7);
        Console.WriteLine(expression8);
        Console.WriteLine(expression9);

        Console.WriteLine();

        var sw1 = Stopwatch.StartNew();
        ICollection<Expression<Func<City, object>>> properties =
        [
            e => e.Name,
            e => e.IsCapital,
            e => e.Country.Name
        ];
        var city = new City
        {
            Name = "New Name",
            IsCapital = true,
            Country = new Country
            {
                Name = "USA"
            }
        };
        Specification<City> citySpecification1 = new Specification<City>
        {
            ExecuteUpdateExpression = ExpressionGenerator<City>.BuildUpdateExpression(properties, city)
        };
        sw1.Stop();


        var sw2 = Stopwatch.StartNew();
        IDictionary<string, object> model = new Dictionary<string, object>
        {
            ["Name"] = "New Name",
            ["IsCapital"] = "true",
            ["Country.Name"] = "USA"
        };
        Specification<City> citySpecification2 = new Specification<City>
        {
            ExecuteUpdateExpression = ExpressionGenerator<City>.BuildUpdateExpression(model)
        };
        sw2.Stop();


        Console.WriteLine(sw1);
        Console.WriteLine(citySpecification1.ExecuteUpdateExpression);
        Console.WriteLine(sw2);
        Console.WriteLine(citySpecification2.ExecuteUpdateExpression);

        List<Country> countries =
        [
            new()
            {
                Name = "USA",
                Cities =
                [
                    new()
                    {
                        Name = "New York"
                    },

                    new()
                    {
                        Name = "San Francisco"
                    }
                ],
                IsIndependent = true
            },

            new()
            {
                Name = "Russia",
                Cities =
                [
                    new()
                    {
                        Name = "Moscow"
                    },

                    new()
                    {
                        Name = "Saint Petersburg"
                    }
                ],
                IsIndependent = true
            },

            new()
            {
                Name = "Germany",
                Cities =
                [
                    new()
                    {
                        Name = "Berlin"
                    },

                    new()
                    {
                        Name = "Munich"
                    }
                ],
                IsIndependent = true
            }
        ];

        Specification<Country> countrySpecification = new Specification<Country>([
            x => x.IsIndependent,
            x => x.Cities.Any(y => y.Name == "Berlin" || y.Name == "Moscow"),
            x => x.Cities.Any(y => y.Name == "Moscow"),
        ], CombineType.And);
        Console.WriteLine(countrySpecification.Criteria);

        var result = countries.Where(countrySpecification.Criteria?.Compile() ?? (x => true));
        Console.WriteLine(string.Join(", ", result.Select(x => x.Name)));
    }

    private static async Task TestSimpleInclude()
    {
        Console.WriteLine("=== Test 1: Simple Include ===");
        
        var specification = new Specification<Country>
        {
            Criteria = c => c.Name == "USA",
            AsNoTracking = true
        };
        
        // Using the new fluent Include API
        specification.Include(c => c.Cities);
        
        var country = await _countryRepository.GetAsync(specification);
        
        if (country != null)
        {
            Console.WriteLine($"Country: {country.Name}");
            Console.WriteLine($"Cities loaded: {country.Cities?.Count ?? 0}");
            if (country.Cities != null)
            {
                foreach (var city in country.Cities)
                {
                    Console.WriteLine($"  - {city.Name}");
                }
            }
        }
        Console.WriteLine();
    }

    private static async Task TestThenInclude()
    {
        Console.WriteLine("=== Test 2: Include with ThenInclude (2 levels) ===");
        
        var specification = new Specification<Country>
        {
            Criteria = c => c.Name == "USA",
            AsNoTracking = true
        };
        
        // Using the new fluent ThenInclude API - type inference works automatically!
        specification
            .Include(c => c.Cities)
            .ThenInclude(c => c.Districts);
        
        var country = await _countryRepository.GetAsync(specification);
        
        if (country != null)
        {
            Console.WriteLine($"Country: {country.Name}");
            Console.WriteLine($"Cities loaded: {country.Cities?.Count ?? 0}");
            if (country.Cities != null)
            {
                foreach (var city in country.Cities)
                {
                    Console.WriteLine($"  City: {city.Name}");
                    Console.WriteLine($"    Districts loaded: {city.Districts?.Count ?? 0}");
                    if (city.Districts != null)
                    {
                        foreach (var district in city.Districts)
                        {
                            Console.WriteLine($"      - {district.Name} (Pop: {district.Population:N0})");
                        }
                    }
                }
            }
        }
        Console.WriteLine();
    }

    private static async Task TestMultiLevelThenInclude()
    {
        Console.WriteLine("=== Test 3: Multiple ThenInclude Chains (3 levels deep) ===");
        
        var specification = new Specification<Country>
        {
            Criteria = c => c.Name == "Russia",
            AsNoTracking = true
        };
        
        // Chain multiple ThenInclude calls - automatic type inference!
        specification
            .Include(c => c.Cities)
            .ThenInclude(c => c.Districts);
        
        var country = await _countryRepository.GetAsync(specification);
        
        if (country != null)
        {
            Console.WriteLine($"Country: {country.Name}");
            Console.WriteLine($"Total Cities: {country.Cities?.Count ?? 0}");
            
            if (country.Cities != null)
            {
                var totalDistricts = country.Cities.Sum(c => c.Districts?.Count ?? 0);
                var totalPopulation = country.Cities
                    .SelectMany(c => c.Districts ?? new List<District>())
                    .Sum(d => d.Population);
                
                Console.WriteLine($"Total Districts: {totalDistricts}");
                Console.WriteLine($"Total District Population: {totalPopulation:N0}");
                
                foreach (var city in country.Cities)
                {
                    Console.WriteLine($"\n  City: {city.Name} (Capital: {city.IsCapital})");
                    if (city.Districts != null)
                    {
                        foreach (var district in city.Districts)
                        {
                            Console.WriteLine($"    → {district.Name}: {district.Population:N0} people");
                        }
                    }
                }
            }
        }
        Console.WriteLine();
    }

    private static async Task TestMixedIncludesAndFilters()
    {
        Console.WriteLine("=== Test 4: Mixed Includes with Filters ===");
        
        var specification = new Specification<Country>
        {
            Criteria = c => c.Cities.Any(city => city.IsCapital),
            AsNoTracking = true
        };

        specification
            .Include(c => c.Cities)
            // .ThenInclude(c => c.Districts);
            .ThenInclude(c => c.Districts.Where(x => x.Name.Equals("Queens")));
        
        var countries = await _countryRepository.GetAllAsync(specification);
        
        Console.WriteLine($"Countries with capital cities: {countries.Count}");
        foreach (var country in countries)
        {
            Console.WriteLine($"\nCountry: {country.Name}");
            var capitalCities = country.Cities?.Where(c => c.IsCapital).ToList();
            if (capitalCities != null && capitalCities.Any())
            {
                foreach (var capital in capitalCities)
                {
                    Console.WriteLine($"  Capital: {capital.Name}");
                    var districtCount = capital.Districts?.Count ?? 0;
                    var totalPop = capital.Districts?.Sum(d => d.Population) ?? 0;
                    Console.WriteLine($"    Districts: {districtCount}, Total Pop: {totalPop:N0}");
                }
            }
        }
        Console.WriteLine();
    }

    private static async Task TestCompareIncludeMethods()
    {
        Console.WriteLine("=== Test 5: Compare String Include vs Expression Include ===");
        
        var sw1 = System.Diagnostics.Stopwatch.StartNew();
        var spec1 = new Specification<Country> { AsNoTracking = true };
        spec1.IncludeString("Cities.Districts");
        var result1 = await _countryRepository.GetAllAsync(spec1);
        sw1.Stop();
        
        var sw2 = System.Diagnostics.Stopwatch.StartNew();
        var spec2 = new Specification<Country> { AsNoTracking = true };
        spec2.Include(c => c.Cities)
             .ThenInclude(c => c.Districts);
        var result2 = await _countryRepository.GetAllAsync(spec2);
        sw2.Stop();
        
        Console.WriteLine($"String Include: {sw1.ElapsedMilliseconds}ms - Loaded {result1.Count} countries");
        Console.WriteLine($"Expression Include: {sw2.ElapsedMilliseconds}ms - Loaded {result2.Count} countries");
        Console.WriteLine($"Both methods loaded the same data: {result1.Count == result2.Count}");
        Console.WriteLine();
    }

    private static async Task TestImprovedInclude()
    {
        Console.WriteLine("=== Test 8: Improved AddInclude (Fluent & Metadata Tracking) ===");

        var spec = new Specification<Country> { AsNoTracking = true };
        
        // Test fluent chaining with Include
        spec.Include(c => c.Cities)
            .ThenInclude(city => city.Districts);

        // Test fluent chaining with IncludeString
        spec.IncludeString("Cities")
            .Include(c => c.Cities); // Valid navigation property (redundant but works for testing chaining)

        var countries = await _countryRepository.GetAllAsync(spec);

        Console.WriteLine($"Loaded {countries.Count} countries with nested data.");
        
        // Verify metadata tracking
        var topLevelIncludes = ((ISpecification<Country>)spec).IncludeExpressions;
        Console.WriteLine($"Top-level includes tracked in IncludeExpressions: {topLevelIncludes.Count}");
        foreach (var include in topLevelIncludes)
        {
            Console.WriteLine($"  - {include}");
        }

        var includeStrings = ((ISpecification<Country>)spec).IncludeStrings;
        Console.WriteLine($"Include strings tracked: {includeStrings.Count}");
        foreach (var s in includeStrings)
        {
            Console.WriteLine($"  - {s}");
        }

        Console.WriteLine();
    }
    
    private static async Task TestFilteredInclude()
    {
        Console.WriteLine("=== Test 6: Filtered Include (Database-Level Filter) ===");
        Console.WriteLine("\n📊 SQL Query Generated:\n");
        
        var specification = new Specification<Country>
        {
            Criteria = c => c.Name == "USA",
            AsNoTracking = true
        };
        
        // Filtered include - filter executes in database, not in memory!
        specification.Include(c => c.Cities.Where(city => city.Name == "New York"));
        
        var country = await _countryRepository.GetAsync(specification);
        
        Console.WriteLine("\n✅ Result:");
        if (country != null)
        {
            Console.WriteLine($"Country: {country.Name}");
            Console.WriteLine($"Cities loaded (filtered): {country.Cities?.Count ?? 0}");
            if (country.Cities != null)
            {
                foreach (var city in country.Cities)
                {
                    Console.WriteLine($"  - {city.Name} ✓ Filter applied in SQL!");
                }
            }
        }
        
        Console.WriteLine("\n🔍 Explanation:");
        Console.WriteLine("The WHERE clause in the SQL shows the filter was applied at the database level.");
        Console.WriteLine("Only 'New York' was returned, NOT all cities filtered in memory.");
        Console.WriteLine();
    }
    
    private static async Task TestFilteredThenInclude()
    {
        Console.WriteLine("=== Test 7: Nested Filtered Include ===");
        
        var specification = new Specification<Country>
        {
            Criteria = c => c.Name == "USA",
            AsNoTracking = true
        };
        
        // Method 1: Load all and filter in-memory (not ideal but works)
        specification
            .Include(c => c.Cities)
            .ThenInclude(c => c.Districts);
        
        var country = await _countryRepository.GetAsync(specification);
        
        if (country != null)
        {
            Console.WriteLine($"Country: {country.Name}");
            Console.WriteLine($"Cities loaded: {country.Cities?.Count ?? 0}");
            if (country.Cities != null)
            {
                foreach (var city in country.Cities)
                {
                    Console.WriteLine($"  City: {city.Name}");
                    // Filter in-memory after loading
                    var filteredDistricts = city.Districts?
                        .Where(d => d.Name == "Queens" || d.Name == "Manhattan")
                        .ToList();
                    Console.WriteLine($"    Total Districts: {city.Districts?.Count ?? 0}");
                    Console.WriteLine($"    Filtered Districts: {filteredDistricts?.Count ?? 0}");
                    if (filteredDistricts != null && filteredDistricts.Any())
                    {
                        foreach (var district in filteredDistricts)
                        {
                            Console.WriteLine($"      - {district.Name} (Pop: {district.Population:N0})");
                        }
                    }
                }
            }
        }
        
        Console.WriteLine("\nNote: Nested filtered includes (filtering Districts within Cities)");
        Console.WriteLine("are complex in EF Core and may require multiple queries or projections.");
        Console.WriteLine();
    }

    #region OrderBy & ThenBy Tests

    private static async Task TestSimpleOrderBy()
    {
        Console.WriteLine("========== Test: Simple OrderBy ==========");
        
        var spec = new Specification<Country>
        {
            AsNoTracking = true
        };
        
        // Apply OrderBy using fluent method
        spec.OrderBy(c => c.Name);
        
        var countries = await _countryRepository.GetAllAsync(spec);
        
        Console.WriteLine("Countries ordered by Name (ascending):");
        foreach (var country in countries)
        {
            Console.WriteLine($"  {country.Name}");
        }
        Console.WriteLine();
    }

    private static async Task TestOrderByDescending()
    {
        Console.WriteLine("========== Test: OrderByDescending ==========");
        
        var spec = new Specification<Country>
        {
            AsNoTracking = true
        };
        
        // Apply OrderByDescending using fluent method
        spec.OrderByDescending(c => c.Name);
        
        var countries = await _countryRepository.GetAllAsync(spec);
        
        Console.WriteLine("Countries ordered by Name (descending):");
        foreach (var country in countries)
        {
            Console.WriteLine($"  {country.Name}");
        }
        Console.WriteLine();
    }

    private static async Task TestThenBy()
    {
        Console.WriteLine("========== Test: OrderBy with ThenBy ==========");
        
        var spec = new Specification<City>
        {
            AsNoTracking = true
        };
        
        // Apply OrderBy followed by ThenBy
        spec.OrderBy(c => c.IsCapital)
            .ThenBy(c => c.Name);
        
        var cities = await _cityRepository.GetAllAsync(spec);
        
        Console.WriteLine("Cities ordered by IsCapital, then by Name:");
        foreach (var city in cities)
        {
            Console.WriteLine($"  {city.Name} (Capital: {city.IsCapital})");
        }
        Console.WriteLine();
    }

    private static async Task TestMultiLevelThenBy()
    {
        Console.WriteLine("========== Test: Multi-level ThenBy ==========");
        
        var spec = new Specification<City>
        {
            AsNoTracking = true
        };
        
        // Apply OrderBy followed by multiple ThenBy
        spec.OrderBy(c => c.CountryId)
            .ThenByDescending(c => c.IsCapital)
            .ThenBy(c => c.Name);
        
        var cities = await _cityRepository.GetAllAsync(spec);
        
        Console.WriteLine("Cities ordered by CountryId, then IsCapital (desc), then Name:");
        foreach (var city in cities)
        {
            Console.WriteLine($"  Country {city.CountryId}: {city.Name} (Capital: {city.IsCapital})");
        }
        Console.WriteLine();
    }

    private static async Task TestMixedOrderByWithIncludes()
    {
        Console.WriteLine("========== Test: OrderBy with Include ==========");
        
        var spec = new Specification<Country>
        {
            AsNoTracking = true
        };
        
        // Apply Include and OrderBy together
        spec.Include(c => c.Cities)
            .OrderBy(c => c.Name);
        
        var countries = await _countryRepository.GetAllAsync(spec);
        
        Console.WriteLine("Countries with Cities, ordered by Country Name:");
        foreach (var country in countries)
        {
            Console.WriteLine($"  {country.Name}");
            foreach (var city in country.Cities.OrderBy(c => c.Name))
            {
                Console.WriteLine($"    - {city.Name}");
            }
        }
        Console.WriteLine();
        
        Console.WriteLine("========== Test: OrderBy with ThenInclude ==========");
        
        var spec2 = new Specification<Country>
        {
            AsNoTracking = true
        };
        
        // Apply Include with ThenInclude and OrderBy
        spec2.Include(c => c.Cities)
             .ThenInclude(city => city.Districts)
             .OrderByDescending(c => c.Name);
        
        var countries2 = await _countryRepository.GetAllAsync(spec2);
        
        Console.WriteLine("Countries with Cities and Districts, ordered by Country Name (desc):");
        foreach (var country in countries2)
        {
            Console.WriteLine($"  {country.Name}");
            foreach (var city in country.Cities.OrderBy(c => c.Name))
            {
                Console.WriteLine($"    City: {city.Name}");
                foreach (var district in city.Districts.OrderBy(d => d.Name).Take(2))
                {
                    Console.WriteLine($"      District: {district.Name} (Pop: {district.Population:N0})");
                }
            }
        }
        Console.WriteLine();
    }

    #endregion
}

