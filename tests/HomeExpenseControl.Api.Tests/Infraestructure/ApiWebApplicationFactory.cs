using System.Data;
using System.Data.Common;
using HomeExpenseControl.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HomeExpenseControl.Api.Tests.Infrastructure;

/// <summary>
/// Inicializa a API com um banco SQLite exclusivo em memória,
/// isolado do banco utilizado durante o desenvolvimento.
/// </summary>
public sealed class ApiWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection =
        new("Data Source=:memory:");

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Evita avisos do middleware de redirecionamento HTTPS
        // ao executar a aplicação por meio do TestServer.
        builder.UseSetting("https_port", "443");

        builder.ConfigureServices(services =>
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            // Remove a configuração SQLite utilizada pela aplicação
            // para substituí-la pelo banco exclusivo dos testes.
            services.RemoveAll<
                DbContextOptions<AppDbContext>>();

            services.RemoveAll<AppDbContext>();

            services.AddSingleton<DbConnection>(
                _connection);

            services.AddDbContext<AppDbContext>(
                (serviceProvider, options) =>
                {
                    var connection = serviceProvider
                        .GetRequiredService<DbConnection>();

                    options.UseSqlite(connection);
                });

            services.AddHostedService<
                TestDatabaseInitializer>();
        });
    }

    /// <summary>
    /// Apaga e recria o banco, garantindo isolamento entre testes.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var scope =
            Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}