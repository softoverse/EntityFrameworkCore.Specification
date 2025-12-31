using System.Diagnostics;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

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
        Console.Clear();
        await InitializeDbContextAsync();
        await SeedDatabase();
        await TestUpdateOperation();
    }

    private static async Task InitializeDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                      .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SpecificationPatternTest;Trusted_Connection=True;")
                      .Options;

        _context = new ApplicationDbContext(options);
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
                        Name = "New York"
                    },
                    new()
                    {
                        Name = "San Francisco"
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
                        Name = "Moscow"
                    },
                    new()
                    {
                        Name = "Saint Petersburg"
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
}