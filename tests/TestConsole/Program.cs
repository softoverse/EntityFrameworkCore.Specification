using System.Diagnostics;
using System.Linq.Expressions;

using Softoverse.EntityFrameworkCore.Specification.Extensions;
using Softoverse.EntityFrameworkCore.Specification.Helpers;
using Softoverse.EntityFrameworkCore.Specification.Implementation;

using TestConsole;

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

List<Country> countries = new List<Country>
{
    new Country()
    {
        Name = "USA",
        Cities = new List<City>()
        {
            new City()
            {
                Name = "New York"
            },
            new City()
            {
                Name = "San Francisco"
            }
        },
        IsIndependent = true
    },
    new Country()
    {
        Name = "Russia",
        Cities = new List<City>()
        {
            new City()
            {
                Name = "Moscow"
            },
            new City()
            {
                Name = "Saint Petersburg"
            }
        },
        IsIndependent = true
    },
    new Country()
    {
        Name = "Germany",
        Cities = new List<City>()
        {
            new City()
            {
                Name = "Berlin"
            },
            new City()
            {
                Name = "Munich"
            }
        },
        IsIndependent = true
    }
};

Specification<Country> countrySpecification = new Specification<Country>([
    x => x.IsIndependent,
    x => x.Cities.Any(y => y.Name == "Berlin" || y.Name == "Moscow"),
    x => x.Cities.Any(y => y.Name == "Moscow"),
], CombineType.And);
Console.WriteLine(countrySpecification.Criteria);

var result = countries.Where(countrySpecification.Criteria?.Compile() ?? (x => true));
Console.WriteLine(string.Join(", ", result.Select(x => x.Name)));