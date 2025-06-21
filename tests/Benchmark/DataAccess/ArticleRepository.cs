

using Benchmark.Models;

namespace Benchmark.DataAccess;

public interface IArticleRepository : IRepository<Article, long>
{

}

public class ArticleRepository(ApplicationDbContext dbContext)
    : GenericRepository<Article, long>(dbContext),
      IArticleRepository;