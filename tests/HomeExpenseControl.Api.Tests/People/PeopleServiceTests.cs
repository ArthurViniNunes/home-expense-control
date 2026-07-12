using HomeExpenseControl.Api.Domain.Entities;
using HomeExpenseControl.Api.Features.People;
using HomeExpenseControl.Api.Features.People.Contracts;
using HomeExpenseControl.Api.Infrastructure.Persistence;
using HomeExpenseControl.Api.Domain.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HomeExpenseControl.Api.Tests.People;

public sealed class PeopleServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistPerson_WhenRequestIsValid()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var service = new PeopleService(context);

        var request = new CreatePersonRequest
        {
            Name = "  Arthur Nunes  ",
            Age = 22
        };

        var result = await service.CreateAsync(
            request,
            CancellationToken.None);

        var persistedPerson = await context.People.SingleAsync();

        Assert.True(result.Id > 0);
        Assert.Equal("Arthur Nunes", result.Name);
        Assert.Equal(22, result.Age);
        Assert.False(result.IsMinor);

        Assert.Equal(result.Id, persistedPerson.Id);
        Assert.Equal("Arthur Nunes", persistedPerson.Name);
        Assert.Equal(22, persistedPerson.Age);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnPeopleOrderedByNameAndId()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        context.People.AddRange(
            new Person("Carlos", 40),
            new Person("Ana", 30),
            new Person("Ana", 17));

        await context.SaveChangesAsync();

        var service = new PeopleService(context);

        var result = await service.ListAsync(
            CancellationToken.None);

        Assert.Equal(3, result.Count);

        Assert.Equal(
            ["Ana", "Ana", "Carlos"],
            result.Select(person => person.Name));

        Assert.True(result[0].Id < result[1].Id);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnEmptyList_WhenNoPeopleExist()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var service = new PeopleService(context);

        var result = await service.ListAsync(
            CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPerson_WhenPersonExists()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var person = new Person("Marcos", 35);

        context.People.Add(person);
        await context.SaveChangesAsync();

        var service = new PeopleService(context);

        var result = await service.GetByIdAsync(
            person.Id,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(person.Id, result.Id);
        Assert.Equal("Marcos", result.Name);
        Assert.Equal(35, result.Age);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenPersonDoesNotExist()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var service = new PeopleService(context);

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

    [Fact]
    public async Task DeleteAsync_ShouldRemovePerson_WhenPersonExists()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var person = new Person("Arthur Nunes", 22);

        context.People.Add(person);
        await context.SaveChangesAsync();

        var service = new PeopleService(context);

        var result = await service.DeleteAsync(
            person.Id,
            CancellationToken.None);

        var personStillExists = await context.People
            .AnyAsync(existingPerson => existingPerson.Id == person.Id);

        Assert.True(result);
        Assert.False(personStillExists);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenPersonDoesNotExist()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var service = new PeopleService(context);

        var result = await service.DeleteAsync(
            999,
            CancellationToken.None);

        Assert.False(result);
        Assert.Empty(await context.People.ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeletePersonTransactions_WhenPersonExists()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var person = new Person("Arthur Nunes", 22);

        var transaction = new Transaction(
            "Conta de energia",
            150m,
            TransactionType.Expense,
            person);

        context.People.Add(person);
        context.Transactions.Add(transaction);

        await context.SaveChangesAsync();

        var personId = person.Id;
        var transactionId = transaction.Id;

        // Limpa as entidades rastreadas para garantir que a exclusão seja
        // executada pela configuração de cascata do banco de dados.
        context.ChangeTracker.Clear();

        var service = new PeopleService(context);

        var result = await service.DeleteAsync(
            personId,
            CancellationToken.None);

        var personExists = await context.People
            .AnyAsync(item => item.Id == personId);

        var transactionExists = await context.Transactions
            .AnyAsync(item => item.Id == transactionId);

        Assert.True(result);
        Assert.False(personExists);
        Assert.False(transactionExists);
    }
}