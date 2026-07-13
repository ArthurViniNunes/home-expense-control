using HomeExpenseControl.Api.Common.Errors;
using HomeExpenseControl.Api.Domain.Entities;
using HomeExpenseControl.Api.Domain.Enums;
using HomeExpenseControl.Api.Features.Transactions;
using HomeExpenseControl.Api.Features.Transactions.Contracts;
using HomeExpenseControl.Api.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HomeExpenseControl.Api.Tests.Transactions;

public sealed class TransactionsServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistExpense_WhenRequestIsValid()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var person = new Person("Arthur Nunes", 22);
        context.People.Add(person);
        await context.SaveChangesAsync();

        var service = new TransactionsService(context);

        var request = new CreateTransactionRequest
        {
            Description = "  Conta de energia  ",
            Amount = 125.90m,
            Type = TransactionType.Expense,
            PersonId = person.Id
        };

        var result = await service.CreateAsync(
            request,
            CancellationToken.None);

        var persistedTransaction = await context.Transactions.SingleAsync();

        Assert.True(result.Id > 0);
        Assert.Equal("Conta de energia", result.Description);
        Assert.Equal(125.90m, result.Amount);
        Assert.Equal(TransactionType.Expense, result.Type);
        Assert.Equal(person.Id, result.Person.Id);
        Assert.Equal("Arthur Nunes", result.Person.Name);

        Assert.Equal(result.Id, persistedTransaction.Id);
        Assert.Equal(12590, persistedTransaction.AmountInCents);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowResourceNotFoundException_WhenPersonDoesNotExist()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var service = new TransactionsService(context);

        var request = new CreateTransactionRequest
        {
            Description = "Conta de energia",
            Amount = 100m,
            Type = TransactionType.Expense,
            PersonId = 999
        };

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => service.CreateAsync(
                request,
                CancellationToken.None));

        Assert.Contains("999", exception.Message);
        Assert.Empty(context.Transactions);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenMinorRegistersIncome()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var person = new Person("Pedro Souza", 17);
        context.People.Add(person);
        await context.SaveChangesAsync();

        var service = new TransactionsService(context);

        var request = new CreateTransactionRequest
        {
            Description = "Mesada",
            Amount = 100m,
            Type = TransactionType.Income,
            PersonId = person.Id
        };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateAsync(
                request,
                CancellationToken.None));

        Assert.Empty(context.Transactions);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnTransactionsWithPersonData()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var person = new Person("Carlos Souza", 35);

        context.People.Add(person);
        await context.SaveChangesAsync();

        context.Transactions.AddRange(
            new Transaction(
                "Salário",
                3000m,
                TransactionType.Income,
                person),
            new Transaction(
                "Aluguel",
                1200m,
                TransactionType.Expense,
                person));

        await context.SaveChangesAsync();

        var service = new TransactionsService(context);

        var result = await service.ListAsync(
            CancellationToken.None);

        Assert.Equal(2, result.Count);

        Assert.Equal("Salário", result[0].Description);
        Assert.Equal(3000m, result[0].Amount);
        Assert.Equal(TransactionType.Income, result[0].Type);

        Assert.Equal("Aluguel", result[1].Description);
        Assert.Equal(1200m, result[1].Amount);
        Assert.Equal(TransactionType.Expense, result[1].Type);

        Assert.All(
            result,
            transaction =>
            {
                Assert.Equal(person.Id, transaction.Person.Id);
                Assert.Equal("Carlos Souza", transaction.Person.Name);
            });
    }

    [Fact]
    public async Task ListAsync_ShouldReturnEmptyList_WhenNoTransactionsExist()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var service = new TransactionsService(context);

        var result = await service.ListAsync(
            CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTransactionDoesNotExist()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var service = new TransactionsService(context);

        var result = await service.GetByIdAsync(
            999,
            CancellationToken.None);

        Assert.Null(result);
    }

    private static async Task<SqliteConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        return connection;
    }

    private static async Task<AppDbContext> CreateContextAsync(
        SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        await context.Database.EnsureCreatedAsync();

        return context;
    }
}