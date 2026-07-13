using HomeExpenseControl.Api.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HomeExpenseControl.Api.Tests.Infrastructure;

public sealed class DatabaseMigrationTests
{
    [Fact]
    public async Task Migrations_ShouldCreateDatabaseSchema_FromEmptyDatabase()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var context =
            new AppDbContext(options);

        await context.Database.MigrateAsync();

        var pendingMigrations = await context.Database
            .GetPendingMigrationsAsync();

        Assert.Empty(pendingMigrations);

        var tables = await GetTableNamesAsync(connection);

        Assert.Contains("People", tables);
        Assert.Contains("Transactions", tables);
        Assert.Contains("__EFMigrationsHistory", tables);
    }

    private static async Task<IReadOnlyList<string>>
        GetTableNamesAsync(SqliteConnection connection)
    {
        await using var command =
            connection.CreateCommand();

        command.CommandText = """
            SELECT name
            FROM sqlite_master
            WHERE type = 'table'
            ORDER BY name;
            """;

        var tableNames = new List<string>();

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tableNames.Add(reader.GetString(0));
        }

        return tableNames;
    }
}