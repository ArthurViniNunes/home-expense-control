using HomeExpenseControl.Api.Domain.Entities;
using HomeExpenseControl.Api.Domain.Enums;
using HomeExpenseControl.Api.Features.Totals;
using HomeExpenseControl.Api.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HomeExpenseControl.Api.Tests.Totals;

public sealed class TotalsServiceTests
{
    [Fact]
    public async Task GetAsync_ShouldReturnEmptyPeopleAndZeroGeneralTotals_WhenNoPeopleExist()
    {
        await using var connection =
            await CreateOpenConnectionAsync();

        await using var context =
            await CreateContextAsync(connection);

        var service = new TotalsService(context);

        var result = await service.GetAsync(
            CancellationToken.None);

        Assert.Empty(result.People);
        Assert.Equal(0m, result.General.TotalIncome);
        Assert.Equal(0m, result.General.TotalExpenses);
        Assert.Equal(0m, result.General.NetBalance);
    }

    [Fact]
    public async Task GetAsync_ShouldIncludePersonWithZeroTotals_WhenPersonHasNoTransactions()
    {
        await using var connection =
            await CreateOpenConnectionAsync();

        await using var context =
            await CreateContextAsync(connection);

        var person = new Person("Arthur Nunes", 28);

        context.People.Add(person);
        await context.SaveChangesAsync();

        var service = new TotalsService(context);

        var result = await service.GetAsync(
            CancellationToken.None);

        var personTotals = Assert.Single(result.People);

        Assert.Equal(person.Id, personTotals.PersonId);
        Assert.Equal("Arthur Nunes", personTotals.PersonName);
        Assert.Equal(0m, personTotals.TotalIncome);
        Assert.Equal(0m, personTotals.TotalExpenses);
        Assert.Equal(0m, personTotals.Balance);

        Assert.Equal(0m, result.General.TotalIncome);
        Assert.Equal(0m, result.General.TotalExpenses);
        Assert.Equal(0m, result.General.NetBalance);
    }

    [Fact]
    public async Task GetAsync_ShouldCalculateIndividualTotals_WhenPersonHasIncomeAndExpenses()
    {
        await using var connection =
            await CreateOpenConnectionAsync();

        await using var context =
            await CreateContextAsync(connection);

        var person = new Person("Carlos Souza", 35);

        context.People.Add(person);

        context.Transactions.AddRange(
            new Transaction(
                "Salário",
                3500m,
                TransactionType.Income,
                person),
            new Transaction(
                "Aluguel",
                1200m,
                TransactionType.Expense,
                person),
            new Transaction(
                "Conta de energia",
                150.50m,
                TransactionType.Expense,
                person));

        await context.SaveChangesAsync();

        var service = new TotalsService(context);

        var result = await service.GetAsync(
            CancellationToken.None);

        var personTotals = Assert.Single(result.People);

        Assert.Equal(3500m, personTotals.TotalIncome);
        Assert.Equal(1350.50m, personTotals.TotalExpenses);
        Assert.Equal(2149.50m, personTotals.Balance);

        Assert.Equal(3500m, result.General.TotalIncome);
        Assert.Equal(1350.50m, result.General.TotalExpenses);
        Assert.Equal(2149.50m, result.General.NetBalance);
    }

    [Fact]
    public async Task GetAsync_ShouldCalculateIndividualAndGeneralTotals_ForMultiplePeople()
    {
        await using var connection =
            await CreateOpenConnectionAsync();

        await using var context =
            await CreateContextAsync(connection);

        var arthur = new Person("Arthur Nunes", 22);
        var carlos = new Person("Carlos Souza", 40);
        var pedro = new Person("Pedro Souza", 16);

        context.People.AddRange(
            arthur,
            carlos,
            pedro);

        context.Transactions.AddRange(
            new Transaction(
                "Salário Arthur",
                3000m,
                TransactionType.Income,
                arthur),
            new Transaction(
                "Aluguel Arthur",
                1200m,
                TransactionType.Expense,
                arthur),
            new Transaction(
                "Salário Carlos",
                2500m,
                TransactionType.Income,
                carlos),
            new Transaction(
                "Despesas Carlos",
                3000m,
                TransactionType.Expense,
                carlos),
            new Transaction(
                "Material escolar",
                200m,
                TransactionType.Expense,
                pedro));

        await context.SaveChangesAsync();

        var service = new TotalsService(context);

        var result = await service.GetAsync(
            CancellationToken.None);

        Assert.Equal(3, result.People.Count);

        var arthurTotals = result.People.Single(
            item => item.PersonId == arthur.Id);

        Assert.Equal(3000m, arthurTotals.TotalIncome);
        Assert.Equal(1200m, arthurTotals.TotalExpenses);
        Assert.Equal(1800m, arthurTotals.Balance);

        var carlosTotals = result.People.Single(
            item => item.PersonId == carlos.Id);

        Assert.Equal(2500m, carlosTotals.TotalIncome);
        Assert.Equal(3000m, carlosTotals.TotalExpenses);
        Assert.Equal(-500m, carlosTotals.Balance);

        var pedroTotals = result.People.Single(
            item => item.PersonId == pedro.Id);

        Assert.Equal(0m, pedroTotals.TotalIncome);
        Assert.Equal(200m, pedroTotals.TotalExpenses);
        Assert.Equal(-200m, pedroTotals.Balance);

        Assert.Equal(5500m, result.General.TotalIncome);
        Assert.Equal(4400m, result.General.TotalExpenses);
        Assert.Equal(1100m, result.General.NetBalance);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnPeopleOrderedByNameAndId()
    {
        await using var connection =
            await CreateOpenConnectionAsync();

        await using var context =
            await CreateContextAsync(connection);

        context.People.AddRange(
            new Person("Carlos", 30),
            new Person("Arthur", 22),
            new Person("Bruna", 28));

        await context.SaveChangesAsync();

        var service = new TotalsService(context);

        var result = await service.GetAsync(
            CancellationToken.None);

        Assert.Equal(
            ["Arthur", "Bruna", "Carlos"],
            result.People.Select(
                item => item.PersonName));
    }

    private static async Task<SqliteConnection>
        CreateOpenConnectionAsync()
    {
        var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        return connection;
    }

    private static async Task<AppDbContext>
        CreateContextAsync(SqliteConnection connection)
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

        var context = new AppDbContext(options);

        await context.Database.EnsureCreatedAsync();

        return context;
    }
}