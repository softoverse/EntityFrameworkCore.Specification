using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TestConsole.DataAccess;

public static class DatabaseInitializer
{
    /// <summary>
    /// Ensure required tables for ApplicationDbContext exist. This runs plain SQL CREATE TABLE statements
    /// guarded with IF OBJECT_ID(...) IS NULL so they are safe to run multiple times.
    /// Target DB: SQL Server.
    /// </summary>
    public static async Task EnsureTablesCreatedAsync(ApplicationDbContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var createCountries = @"
IF OBJECT_ID(N'dbo.Countries','U') IS NULL
BEGIN
    CREATE TABLE dbo.Countries (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RowId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(MAX) NOT NULL,
        IsIndependent BIT NOT NULL,
        Population DECIMAL(18,2) NOT NULL
    );
END
";

        var createCities = @"
IF OBJECT_ID(N'dbo.Cities','U') IS NULL
BEGIN
    CREATE TABLE dbo.Cities (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RowId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(MAX) NOT NULL,
        IsCapital BIT NOT NULL,
        CountryId BIGINT NOT NULL
    );
    ALTER TABLE dbo.Cities
    ADD CONSTRAINT FK_Cities_Countries_CountryId FOREIGN KEY (CountryId) REFERENCES dbo.Countries (Id);
END
";

        // Execute DDL. Open connection explicitly to avoid EF attempting to create DB or run migrations.
        await context.Database.OpenConnectionAsync();
        try
        {
            await context.Database.ExecuteSqlRawAsync(createCountries);
            await context.Database.ExecuteSqlRawAsync(createCities);
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }
}
