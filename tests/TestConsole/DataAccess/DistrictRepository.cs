

using TestConsole.Models;

namespace TestConsole.DataAccess;

public interface ICountryRepository : IRepository<Country, long>
{

}

public class CountryRepository(ApplicationDbContext dbContext)
    : GenericRepository<Country, long>(dbContext),
      ICountryRepository;