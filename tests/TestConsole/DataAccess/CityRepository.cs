

using TestConsole.Models;

namespace TestConsole.DataAccess;

public interface ICityRepository : IRepository<City, long>
{

}

public class CityRepository(ApplicationDbContext dbContext)
    : GenericRepository<City, long>(dbContext),
      ICityRepository;