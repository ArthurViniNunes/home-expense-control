using HomeExpenseControl.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomeExpenseControl.Api.Tests.Infrastructure;

/// <summary>
/// Cria o schema do banco SQLite em memória antes que a API
/// comece a receber requisições durante os testes.
/// </summary>
internal sealed class TestDatabaseInitializer
    : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TestDatabaseInitializer(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(
        CancellationToken cancellationToken)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureCreatedAsync(
            cancellationToken);
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}