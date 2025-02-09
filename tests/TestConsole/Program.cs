// See https://aka.ms/new-console-template for more information

using Softoverse.EntityFrameworkCore.Specification.Abstraction;
using Softoverse.EntityFrameworkCore.Specification.Implementation;

using TestConsole;

Console.WriteLine("Hello, World!");

Specification<Country> specification =  new Specification<Country>(1, true, true);

ISpecification<Country> iSpecification = specification;

// iSpecification.ExecuteUpdateProperties.Count;
// iSpecification.Add

