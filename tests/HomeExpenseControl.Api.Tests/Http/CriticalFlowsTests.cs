using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeExpenseControl.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HomeExpenseControl.Api.Tests.Http;

public sealed class CriticalFlowsTests
    : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CriticalFlowsTests(
        ApiWebApplicationFactory factory)
    {
        _factory = factory;

        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress =
                    new Uri("https://localhost")
            });
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnCreated_WithPersistedPerson()
    {
        await _factory.ResetDatabaseAsync();

        using var response = await _client.PostAsJsonAsync(
            "/api/people",
            new
            {
                name = "Ana Souza",
                age = 28
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        Assert.NotNull(response.Headers.Location);

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var person = document.RootElement;

        Assert.True(
            person.GetProperty("id").GetInt32() > 0);

        Assert.Equal(
            "Ana Souza",
            person.GetProperty("name").GetString());

        Assert.Equal(
            28,
            person.GetProperty("age").GetInt32());

        Assert.False(
            person.GetProperty("isMinor").GetBoolean());
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnNotFound_WhenPersonDoesNotExist()
    {
        await _factory.ResetDatabaseAsync();

        using var response =
            await _client.PostAsJsonAsync(
                "/api/transactions",
                new
                {
                    description = "Conta de energia",
                    amount = 145.34m,
                    type = "expense",
                    personId = 999
                });

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        Assert.Equal(
            "Recurso não encontrado",
            document.RootElement
                .GetProperty("title")
                .GetString());

        Assert.Contains(
            "999",
            document.RootElement
                .GetProperty("detail")
                .GetString());
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnUnprocessableEntity_WhenMinorRegistersIncome()
    {
        await _factory.ResetDatabaseAsync();

        var minorId = await CreatePersonAsync(
            "Pedro Souza",
            17);

        using var response =
            await _client.PostAsJsonAsync(
                "/api/transactions",
                new
                {
                    description = "Mesada",
                    amount = 100m,
                    type = "income",
                    personId = minorId
                });

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            response.StatusCode);

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        Assert.Equal(
            "Regra de negócio violada",
            document.RootElement
                .GetProperty("title")
                .GetString());

        Assert.Contains(
            "menores de 18 anos",
            document.RootElement
                .GetProperty("detail")
                .GetString(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeletePerson_ShouldDeleteAssociatedTransactions()
    {
        await _factory.ResetDatabaseAsync();

        var personId = await CreatePersonAsync(
            "Carlos Souza",
            35);

        var transactionId =
            await CreateTransactionAsync(
                "Conta de água",
                120.50m,
                "expense",
                personId);

        using var deleteResponse =
            await _client.DeleteAsync(
                $"/api/people/{personId}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        using var transactionResponse =
            await _client.GetAsync(
                $"/api/transactions/{transactionId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            transactionResponse.StatusCode);
    }

    [Fact]
    public async Task GetTotals_ShouldReturnIndividualAndGeneralTotals()
    {
        await _factory.ResetDatabaseAsync();

        var anaId = await CreatePersonAsync(
            "Ana Souza",
            28);

        var pedroId = await CreatePersonAsync(
            "Pedro Souza",
            16);

        await CreateTransactionAsync(
            "Salário",
            3000m,
            "income",
            anaId);

        await CreateTransactionAsync(
            "Aluguel",
            1200.50m,
            "expense",
            anaId);

        await CreateTransactionAsync(
            "Material escolar",
            200m,
            "expense",
            pedroId);

        using var response =
            await _client.GetAsync("/api/totals");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var root = document.RootElement;

        var people = root
            .GetProperty("people")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(2, people.Length);

        var ana = people.Single(person =>
            person.GetProperty("personId").GetInt32()
            == anaId);

        Assert.Equal(
            3000m,
            ana.GetProperty("totalIncome").GetDecimal());

        Assert.Equal(
            1200.50m,
            ana.GetProperty("totalExpenses").GetDecimal());

        Assert.Equal(
            1799.50m,
            ana.GetProperty("balance").GetDecimal());

        var pedro = people.Single(person =>
            person.GetProperty("personId").GetInt32()
            == pedroId);

        Assert.Equal(
            0m,
            pedro.GetProperty("totalIncome").GetDecimal());

        Assert.Equal(
            200m,
            pedro.GetProperty("totalExpenses").GetDecimal());

        Assert.Equal(
            -200m,
            pedro.GetProperty("balance").GetDecimal());

        var general = root.GetProperty("general");

        Assert.Equal(
            3000m,
            general
                .GetProperty("totalIncome")
                .GetDecimal());

        Assert.Equal(
            1400.50m,
            general
                .GetProperty("totalExpenses")
                .GetDecimal());

        Assert.Equal(
            1599.50m,
            general
                .GetProperty("netBalance")
                .GetDecimal());
    }

    private async Task<int> CreatePersonAsync(
        string name,
        int age)
    {
        using var response =
            await _client.PostAsJsonAsync(
                "/api/people",
                new
                {
                    name,
                    age
                });

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        return document.RootElement
            .GetProperty("id")
            .GetInt32();
    }

    private async Task<int> CreateTransactionAsync(
        string description,
        decimal amount,
        string type,
        int personId)
    {
        using var response =
            await _client.PostAsJsonAsync(
                "/api/transactions",
                new
                {
                    description,
                    amount,
                    type,
                    personId
                });

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        return document.RootElement
            .GetProperty("id")
            .GetInt32();
    }
}