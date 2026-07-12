using HomeExpenseControl.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeExpenseControl.Api.Infrastructure.Persistence;

/// <summary>
/// Representa a sessão de comunicação da aplicação com o banco de dados.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> People => Set<Person>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Localiza automaticamente as configurações de entidades
        // existentes no assembly da aplicação.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}