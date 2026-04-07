

using TestConsole.Models;

namespace TestConsole.DataAccess;

public interface IDistrictRepository : IRepository<District, long>
{

}

public class DistrictRepository(ApplicationDbContext dbContext)
    : GenericRepository<District, long>(dbContext),
      IDistrictRepository;