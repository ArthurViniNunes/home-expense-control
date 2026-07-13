using Microsoft.EntityFrameworkCore;

namespace HomeExpenseControl.Api.Infrastructure.Persistence;

/// <summary>
/// Disponibiliza a inicialização automática do banco de dados
/// por meio das migrations do Entity Framework Core.
/// </summary>
public static class DatabaseMigrationExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(
        this WebApplication app)
    {
        var shouldApplyMigrations = app.Configuration
            .GetValue<bool>(
                "Database:ApplyMigrationsOnStartup");

        if (!shouldApplyMigrations)
        {
            return;
        }

        await using var scope =
            app.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var pendingMigrations = await dbContext.Database
            .GetPendingMigrationsAsync();

        var migrations = pendingMigrations.ToArray();

        if (migrations.Length == 0)
        {
            app.Logger.LogInformation(
                "O banco de dados já está atualizado.");

            return;
        }

        app.Logger.LogInformation(
            "Aplicando {MigrationCount} migration(s): {Migrations}.",
            migrations.Length,
            string.Join(", ", migrations));

        await dbContext.Database.MigrateAsync();

        app.Logger.LogInformation(
            "Migrations aplicadas com sucesso.");
    }
}