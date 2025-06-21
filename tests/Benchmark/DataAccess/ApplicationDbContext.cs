

using Benchmark.Models;

using Microsoft.EntityFrameworkCore;

namespace Benchmark.DataAccess;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Article> Articles { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Configure all enum properties to be stored as strings
        configurationBuilder
            .Properties<Enum>()
            .HaveConversion<string>();

        base.ConfigureConventions(configurationBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // When should you publish domain events?
        //
        // 1. BEFORE calling SaveChangesAsync
        //     - domain events are part of the same transaction
        //     - immediate consistency
        // 2. AFTER calling SaveChangesAsync
        //     - domain events are a separate transaction
        //     - eventual consistency
        //     - handlers can fail

        int result;
        try
        {
            result = await base.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw;
        }

        return result;
    }
}