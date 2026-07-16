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

    [Fact]
    public async Task ListTransactions_ShouldApplyCombinedFiltersFromQueryString()
    {
        var adultPersonId = await CreatePersonAsync(
            "Adulto dos filtros",
            30);

        var minorPersonId = await CreatePersonAsync(
            "Menor dos filtros",
            17);

        await CreateTransactionAsync(
            "Despesa dentro da faixa",
            150m,
            "expense",
            adultPersonId);

        await CreateTransactionAsync(
            "Despesa abaixo da faixa",
            50m,
            "expense",
            adultPersonId);

        await CreateTransactionAsync(
            "Despesa acima da faixa",
            500m,
            "expense",
            adultPersonId);

        await CreateTransactionAsync(
            "Receita do adulto",
            150m,
            "income",
            adultPersonId);

        await CreateTransactionAsync(
            "Despesa do menor",
            150m,
            "expense",
            minorPersonId);

        using var response = await _client.GetAsync(
            "/api/transactions" +
            $"?personId={adultPersonId}" +
            "&ageGroup=adult" +
            "&type=expense" +
            "&minAmount=100" +
            "&maxAmount=200");

        response.EnsureSuccessStatusCode();

        await using var responseStream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(responseStream);

        var transactions = document.RootElement;

        Assert.Equal(
            JsonValueKind.Array,
            transactions.ValueKind);

        Assert.Equal(
            1,
            transactions.GetArrayLength());

        var transaction = transactions[0];

        Assert.Equal(
            "Despesa dentro da faixa",
            transaction
                .GetProperty("description")
                .GetString());

        Assert.Equal(
            150m,
            transaction
                .GetProperty("amount")
                .GetDecimal());

        Assert.Equal(
            "expense",
            transaction
                .GetProperty("type")
                .GetString());

        Assert.Equal(
            adultPersonId,
            transaction
                .GetProperty("person")
                .GetProperty("id")
                .GetInt32());
    }

    [Fact]
    public async Task UpdateTransaction_ShouldReturnNotFound_WhenTransactionDoesNotExist()
    {
        var personId =
            await CreatePersonAsync(
                "Pessoa para edição inexistente",
                35);

        using var response =
            await _client.PutAsJsonAsync(
                $"/api/transactions/{int.MaxValue}",
                new
                {
                    description = "Conta atualizada",
                    amount = 200m,
                    type = "expense",
                    personId
                });

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransaction_ShouldReturnNotFound_WhenPersonDoesNotExist()
    {
        var personId =
            await CreatePersonAsync(
                "Pessoa original da edição",
                35);

        var transactionId =
            await CreateTransactionAsync(
                "Conta original",
                125.90m,
                "expense",
                personId);

        using var response =
            await _client.PutAsJsonAsync(
                $"/api/transactions/{transactionId}",
                new
                {
                    description = "Conta atualizada",
                    amount = 200m,
                    type = "expense",
                    personId = int.MaxValue
                });

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        using var persistedResponse =
            await _client.GetAsync(
                $"/api/transactions/{transactionId}");

        persistedResponse.EnsureSuccessStatusCode();

        await using var stream =
            await persistedResponse.Content
                .ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        Assert.Equal(
            "Conta original",
            document.RootElement
                .GetProperty("description")
                .GetString());

        Assert.Equal(
            personId,
            document.RootElement
                .GetProperty("person")
                .GetProperty("id")
                .GetInt32());
    }

    [Fact]
    public async Task DeleteTransaction_ShouldReturnNotFound_WhenTransactionDoesNotExist()
    {
        using var response =
            await _client.DeleteAsync(
                $"/api/transactions/{int.MaxValue}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
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

    [Fact]
    public async Task UpdateTransaction_ShouldPersistChanges_WhenRequestIsValid()
    {
        var originalPersonId =
            await CreatePersonAsync(
                "Carlos da atualização",
                35);

        var newPersonId =
            await CreatePersonAsync(
                "Maria da atualização",
                28);

        var transactionId =
            await CreateTransactionAsync(
                "Conta de energia",
                125.90m,
                "expense",
                originalPersonId);

        using var updateResponse =
            await _client.PutAsJsonAsync(
                $"/api/transactions/{transactionId}",
                new
                {
                    description =
                        "  Salário atualizado  ",
                    amount = 3500m,
                    type = "income",
                    personId = newPersonId
                });

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        await using var updateStream =
            await updateResponse.Content
                .ReadAsStreamAsync();

        using var updateDocument =
            await JsonDocument.ParseAsync(
                updateStream);

        var updatedTransaction =
            updateDocument.RootElement;

        Assert.Equal(
            transactionId,
            updatedTransaction
                .GetProperty("id")
                .GetInt32());

        Assert.Equal(
            "Salário atualizado",
            updatedTransaction
                .GetProperty("description")
                .GetString());

        Assert.Equal(
            3500m,
            updatedTransaction
                .GetProperty("amount")
                .GetDecimal());

        Assert.Equal(
            "income",
            updatedTransaction
                .GetProperty("type")
                .GetString());

        Assert.Equal(
            newPersonId,
            updatedTransaction
                .GetProperty("person")
                .GetProperty("id")
                .GetInt32());

        using var getResponse =
            await _client.GetAsync(
                $"/api/transactions/{transactionId}");

        getResponse.EnsureSuccessStatusCode();

        await using var getStream =
            await getResponse.Content
                .ReadAsStreamAsync();

        using var getDocument =
            await JsonDocument.ParseAsync(
                getStream);

        var persistedTransaction =
            getDocument.RootElement;

        Assert.Equal(
            "Salário atualizado",
            persistedTransaction
                .GetProperty("description")
                .GetString());

        Assert.Equal(
            newPersonId,
            persistedTransaction
                .GetProperty("person")
                .GetProperty("id")
                .GetInt32());
    }

    [Fact]
    public async Task DeleteTransaction_ShouldRemoveTransactionAndKeepPerson()
    {
        var personId =
            await CreatePersonAsync(
                "Carlos da exclusão",
                35);

        var transactionId =
            await CreateTransactionAsync(
                "Conta a excluir",
                125.90m,
                "expense",
                personId);

        using var deleteResponse =
            await _client.DeleteAsync(
                $"/api/transactions/{transactionId}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        using var transactionResponse =
            await _client.GetAsync(
                $"/api/transactions/{transactionId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            transactionResponse.StatusCode);

        using var personResponse =
            await _client.GetAsync(
                $"/api/people/{personId}");

        Assert.Equal(
            HttpStatusCode.OK,
            personResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateTransaction_ShouldReturnUnprocessableEntity_WhenMinorReceivesIncome()
    {
        var adultPersonId =
            await CreatePersonAsync(
                "Adulto da edição",
                35);

        var minorPersonId =
            await CreatePersonAsync(
                "Menor da edição",
                17);

        var transactionId =
            await CreateTransactionAsync(
                "Despesa original",
                100m,
                "expense",
                adultPersonId);

        using var response =
            await _client.PutAsJsonAsync(
                $"/api/transactions/{transactionId}",
                new
                {
                    description = "Receita inválida",
                    amount = 500m,
                    type = "income",
                    personId = minorPersonId
                });

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            response.StatusCode);

        using var persistedResponse =
            await _client.GetAsync(
                $"/api/transactions/{transactionId}");

        persistedResponse.EnsureSuccessStatusCode();

        await using var stream =
            await persistedResponse.Content
                .ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var persistedTransaction =
            document.RootElement;

        Assert.Equal(
            "Despesa original",
            persistedTransaction
                .GetProperty("description")
                .GetString());

        Assert.Equal(
            "expense",
            persistedTransaction
                .GetProperty("type")
                .GetString());

        Assert.Equal(
            adultPersonId,
            persistedTransaction
                .GetProperty("person")
                .GetProperty("id")
                .GetInt32());
    }
}