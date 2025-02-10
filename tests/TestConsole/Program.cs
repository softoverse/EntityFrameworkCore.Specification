// See https://aka.ms/new-console-template for more information

using Softoverse.EntityFrameworkCore.Specification.Extensions;
using Softoverse.EntityFrameworkCore.Specification.Implementation;

using TestConsole;

Console.WriteLine("Hello, World!");

Specification<Country> specification = new Specification<Country>();

var expression1 = specification.ToConditionalExpression(x => x.Name, "BD");

var expression2 = specification.ToConditionalExpression(x => x.Name, "BD", x => x.Name == "BD");

var expression3 = specification.ToConditionalExpression(x => x.Name, "BD", EqualOperation.Equal);

var expression4 = specification.ToConditionalExpression(x => x.IsIndependent, false, EqualOperation.Equal);

var expression5 = specification.ToConditionalExpression(x => x.Population, 0, CompareOperation.GreaterThan);

var expression6 = specification.ToConditionalExpression(x => x.IsIndependent, false, x => x.IsIndependent == false);

var expression7 = specification.ToConditionalExpression(x => x.IsIndependent, "eqqwe:True", EqualOperation.NotEqual);

var expression8 = specification.ToConditionalExpression(x => x.IsIndependent, "eq:true", EqualOperation.NotEqual);

Console.WriteLine(expression1);
Console.WriteLine(expression2);
Console.WriteLine(expression3);
Console.WriteLine(expression4);
Console.WriteLine(expression5);
Console.WriteLine(expression6);
Console.WriteLine(expression7);
Console.WriteLine(expression8);