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

        var person = new Person("Carlos Souza", 17);
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
            new ListTransactionsQuery(),
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
            new ListTransactionsQuery(),
            CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListAsync_ShouldFilterTransactionsByPersonId()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var scenario = await CreateFilterScenarioAsync(context);
        var service = new TransactionsService(context);

        var query = new ListTransactionsQuery
        {
            PersonId = scenario.Arthur.Id
        };

        var result = await service.ListAsync(
            query,
            CancellationToken.None);

        Assert.Equal(3, result.Count);

        Assert.All(
            result,
            transaction =>
                Assert.Equal(
                    scenario.Arthur.Id,
                    transaction.Person.Id));
    }

    [Fact]
    public async Task ListAsync_ShouldFilterTransactionsByType()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        await CreateFilterScenarioAsync(context);

        var service = new TransactionsService(context);

        var query = new ListTransactionsQuery
        {
            Type = "expense"
        };

        var result = await service.ListAsync(
            query,
            CancellationToken.None);

        Assert.Equal(4, result.Count);

        Assert.All(
            result,
            transaction =>
                Assert.Equal(
                    TransactionType.Expense,
                    transaction.Type));
    }

    [Fact]
    public async Task ListAsync_ShouldIncludeMinimumAmountBoundary()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        await CreateFilterScenarioAsync(context);

        var service = new TransactionsService(context);

        var query = new ListTransactionsQuery
        {
            MinAmount = 150m
        };

        var result = await service.ListAsync(
            query,
            CancellationToken.None);

        Assert.Equal(4, result.Count);

        Assert.Contains(
            result,
            transaction =>
                transaction.Amount == 150m);

        Assert.All(
            result,
            transaction =>
                Assert.True(transaction.Amount >= 150m));
    }

    [Fact]
    public async Task ListAsync_ShouldIncludeMaximumAmountBoundary()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        await CreateFilterScenarioAsync(context);

        var service = new TransactionsService(context);

        var query = new ListTransactionsQuery
        {
            MaxAmount = 150m
        };

        var result = await service.ListAsync(
            query,
            CancellationToken.None);

        Assert.Equal(3, result.Count);

        Assert.Contains(
            result,
            transaction =>
                transaction.Amount == 150m);

        Assert.All(
            result,
            transaction =>
                Assert.True(transaction.Amount <= 150m));
    }

    [Fact]
    public async Task ListAsync_ShouldCombineAllTransactionFilters()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var scenario = await CreateFilterScenarioAsync(context);
        var service = new TransactionsService(context);

        var query = new ListTransactionsQuery
        {
            PersonId = scenario.Arthur.Id,
            Type = "expense",
            MinAmount = 100m,
            MaxAmount = 200m
        };

        var result = await service.ListAsync(
            query,
            CancellationToken.None);

        var transaction = Assert.Single(result);

        Assert.Equal(
            "Energia Arthur",
            transaction.Description);

        Assert.Equal(150m, transaction.Amount);

        Assert.Equal(
            TransactionType.Expense,
            transaction.Type);

        Assert.Equal(
            scenario.Arthur.Id,
            transaction.Person.Id);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnEmptyList_WhenFiltersDoNotMatch()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        await CreateFilterScenarioAsync(context);

        var service = new TransactionsService(context);

        var query = new ListTransactionsQuery
        {
            PersonId = 999
        };

        var result = await service.ListAsync(
            query,
            CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListAsync_ShouldFilterTransactionsByMinorAgeGroup()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var scenario = await CreateFilterScenarioAsync(context);
        var service = new TransactionsService(context);

        var query = new ListTransactionsQuery
        {
            AgeGroup = "minor"
        };

        var result = await service.ListAsync(
            query,
            CancellationToken.None);

        var transaction = Assert.Single(result);

        Assert.Equal(
            scenario.Carlos.Id,
            transaction.Person.Id);

        Assert.Equal(
            "Material escolar Carlos",
            transaction.Description);
    }

    [Fact]
    public async Task ListAsync_ShouldFilterTransactionsByAdultAgeGroup()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        await CreateFilterScenarioAsync(context);

        var service = new TransactionsService(context);

        var query = new ListTransactionsQuery
        {
            AgeGroup = "adult"
        };

        var result = await service.ListAsync(
            query,
            CancellationToken.None);

        Assert.Equal(5, result.Count);

        Assert.All(
            result,
            transaction =>
                Assert.NotEqual(
                    "Carlos Souza",
                    transaction.Person.Name));
    }

    [Fact]
    public async Task ListAsync_ShouldCombinePersonAndAgeGroupFilters()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var scenario = await CreateFilterScenarioAsync(context);
        var service = new TransactionsService(context);

        var query = new ListTransactionsQuery
        {
            PersonId = scenario.Carlos.Id,
            AgeGroup = "minor"
        };

        var result = await service.ListAsync(
            query,
            CancellationToken.None);

        var transaction = Assert.Single(result);

        Assert.Equal(
            scenario.Carlos.Id,
            transaction.Person.Id);
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

    private static async Task<FilterScenario> CreateFilterScenarioAsync(
    AppDbContext context)
    {
        var arthur = new Person(
            "Arthur Nunes",
            30);

        var ana = new Person(
            "Ana Souza",
            28);

        var carlos = new Person(
            "Carlos Souza",
            17);

        context.People.AddRange(
            arthur,
            ana,
            carlos);

        await context.SaveChangesAsync();

        context.Transactions.AddRange(
            new Transaction(
                "Salário Arthur",
                3000m,
                TransactionType.Income,
                arthur),
            new Transaction(
                "Energia Arthur",
                150m,
                TransactionType.Expense,
                arthur),
            new Transaction(
                "Mercado Arthur",
                450m,
                TransactionType.Expense,
                arthur),
            new Transaction(
                "Salário Ana",
                2500m,
                TransactionType.Income,
                ana),
            new Transaction(
                "Internet Ana",
                100m,
                TransactionType.Expense,
                ana),
            new Transaction(
                "Material escolar Carlos",
                50m,
                TransactionType.Expense,
                carlos));

        await context.SaveChangesAsync();

        return new FilterScenario(
            arthur,
            ana,
            carlos);
    }

    private sealed record FilterScenario(
        Person Arthur,
        Person Ana,
        Person Carlos);
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
    public async Task UpdateAsync_ShouldPersistUpdatedTransaction_WhenRequestIsValid()
    {
        await using var connection =
            await CreateOpenConnectionAsync();

        await using var context =
            await CreateContextAsync(connection);

        var originalPerson = new Person(
            "Carlos Souza",
            35);

        var newPerson = new Person(
            "Maria Souza",
            28);

        context.People.AddRange(
            originalPerson,
            newPerson);

        await context.SaveChangesAsync();

        var transaction = new Transaction(
            "Conta de energia",
            125.90m,
            TransactionType.Expense,
            originalPerson);

        context.Transactions.Add(transaction);

        await context.SaveChangesAsync();

        var service = new TransactionsService(context);

        var request = new UpdateTransactionRequest
        {
            Description = "  Salário mensal  ",
            Amount = 3500m,
            Type = TransactionType.Income,
            PersonId = newPerson.Id
        };

        var result = await service.UpdateAsync(
            transaction.Id,
            request,
            CancellationToken.None);

        context.ChangeTracker.Clear();

        var persistedTransaction =
            await context.Transactions
                .Include(item => item.Person)
                .SingleAsync();

        Assert.Equal(
            transaction.Id,
            result.Id);

        Assert.Equal(
            "Salário mensal",
            result.Description);

        Assert.Equal(
            3500m,
            result.Amount);

        Assert.Equal(
            TransactionType.Income,
            result.Type);

        Assert.Equal(
            newPerson.Id,
            result.Person.Id);

        Assert.Equal(
            "Salário mensal",
            persistedTransaction.Description);

        Assert.Equal(
            350000,
            persistedTransaction.AmountInCents);

        Assert.Equal(
            TransactionType.Income,
            persistedTransaction.Type);

        Assert.Equal(
            newPerson.Id,
            persistedTransaction.PersonId);

        Assert.Equal(
            "Maria Souza",
            persistedTransaction.Person.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowResourceNotFoundException_WhenTransactionDoesNotExist()
    {
        await using var connection =
            await CreateOpenConnectionAsync();

        await using var context =
            await CreateContextAsync(connection);

        var person = new Person(
            "Carlos Souza",
            35);

        context.People.Add(person);

        await context.SaveChangesAsync();

        var service = new TransactionsService(context);

        var request = new UpdateTransactionRequest
        {
            Description = "Conta atualizada",
            Amount = 200m,
            Type = TransactionType.Expense,
            PersonId = person.Id
        };

        var exception =
            await Assert.ThrowsAsync<ResourceNotFoundException>(
                () => service.UpdateAsync(
                    999,
                    request,
                    CancellationToken.None));

        Assert.Contains(
            "999",
            exception.Message);

        Assert.Empty(context.Transactions);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowResourceNotFoundException_WhenPersonDoesNotExist()
    {
        await using var connection =
            await CreateOpenConnectionAsync();

        await using var context =
            await CreateContextAsync(connection);

        var person = new Person(
            "Carlos Souza",
            35);

        context.People.Add(person);

        await context.SaveChangesAsync();

        var transaction = new Transaction(
            "Conta de energia",
            125.90m,
            TransactionType.Expense,
            person);

        context.Transactions.Add(transaction);

        await context.SaveChangesAsync();

        var service = new TransactionsService(context);

        var request = new UpdateTransactionRequest
        {
            Description = "Conta atualizada",
            Amount = 200m,
            Type = TransactionType.Expense,
            PersonId = 999
        };

        var exception =
            await Assert.ThrowsAsync<ResourceNotFoundException>(
                () => service.UpdateAsync(
                    transaction.Id,
                    request,
                    CancellationToken.None));

        Assert.Contains(
            "999",
            exception.Message);

        context.ChangeTracker.Clear();

        var persistedTransaction =
            await context.Transactions
                .SingleAsync();

        Assert.Equal(
            "Conta de energia",
            persistedTransaction.Description);

        Assert.Equal(
            12590,
            persistedTransaction.AmountInCents);

        Assert.Equal(
            person.Id,
            persistedTransaction.PersonId);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveOnlyTransaction_WhenTransactionExists()
    {
        await using var connection =
            await CreateOpenConnectionAsync();

        await using var context =
            await CreateContextAsync(connection);

        var person = new Person(
            "Carlos Souza",
            35);

        context.People.Add(person);

        await context.SaveChangesAsync();

        var transaction = new Transaction(
            "Conta de energia",
            125.90m,
            TransactionType.Expense,
            person);

        context.Transactions.Add(transaction);

        await context.SaveChangesAsync();

        var service = new TransactionsService(context);

        await service.DeleteAsync(
            transaction.Id,
            CancellationToken.None);

        Assert.Empty(
            await context.Transactions.ToListAsync());

        var persistedPerson =
            await context.People.SingleAsync();

        Assert.Equal(
            person.Id,
            persistedPerson.Id);

        Assert.Equal(
            "Carlos Souza",
            persistedPerson.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowResourceNotFoundException_WhenTransactionDoesNotExist()
    {
        await using var connection =
            await CreateOpenConnectionAsync();

        await using var context =
            await CreateContextAsync(connection);

        var service = new TransactionsService(context);

        var exception =
            await Assert.ThrowsAsync<ResourceNotFoundException>(
                () => service.DeleteAsync(
                    999,
                    CancellationToken.None));

        Assert.Contains(
            "999",
            exception.Message);
    }
}