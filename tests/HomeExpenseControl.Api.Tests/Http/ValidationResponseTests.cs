using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace HomeExpenseControl.Api.Tests.Http;

public sealed class ValidationResponseTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ValidationResponseTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
            })
            .CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost")
                });
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnPortugueseError_WhenTypeIsNumeric()
    {
        const string requestBody = """
            {
              "description": "Teste de transação",
              "amount": 145.34,
              "type": 1,
              "personId": 3
            }
            """;

        using var content = new StringContent(
            requestBody,
            Encoding.UTF8,
            "application/json");

        using var response = await _client.PostAsync(
            "/api/transactions",
            content);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        await using var responseStream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(responseStream);

        var root = document.RootElement;

        Assert.Equal(
            "Dados inválidos",
            root.GetProperty("title").GetString());

        Assert.Equal(
            400,
            root.GetProperty("status").GetInt32());

        var errors = root.GetProperty("errors");

        Assert.True(errors.TryGetProperty("type", out var typeErrors));
        Assert.False(errors.TryGetProperty("request", out _));
        Assert.False(errors.TryGetProperty("$.type", out _));

        Assert.Equal(
            "O tipo deve ser 'expense' para despesa " +
            "ou 'income' para receita.",
            typeErrors[0].GetString());
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnPortugueseError_WhenAgeIsNegative()
    {
        const string requestBody = """
            {
              "name": "Arthur Nunes",
              "age": -1
            }
            """;

        using var content = new StringContent(
            requestBody,
            Encoding.UTF8,
            "application/json");

        using var response = await _client.PostAsync(
            "/api/people",
            content);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        await using var responseStream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(responseStream);

        var ageErrors = document.RootElement
            .GetProperty("errors")
            .GetProperty("age");

        Assert.Equal(
            "A idade não pode ser negativa.",
            ageErrors[0].GetString());
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnBodyError_WhenBodyIsEmpty()
    {
        using var content = new StringContent(
            string.Empty,
            Encoding.UTF8,
            "application/json");

        using var response = await _client.PostAsync(
            "/api/people",
            content);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        await using var responseStream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(responseStream);

        var errors = document.RootElement
            .GetProperty("errors");

        Assert.True(errors.TryGetProperty("body", out var bodyErrors));

        Assert.Equal(
            "O corpo da requisição é obrigatório.",
            bodyErrors[0].GetString());
    }

    [Theory]
    [InlineData(
    "/api/transactions?personId=0",
    "personId",
    "O identificador da pessoa deve ser maior que zero.")]
    [InlineData(
    "/api/transactions?type=other",
    "type",
    "O tipo deve ser expense para despesa ou income para receita.")]
    [InlineData(
    "/api/transactions?type=1",
    "type",
    "O tipo deve ser expense para despesa ou income para receita.")]
    [InlineData(
    "/api/transactions?ageGroup=child",
    "ageGroup",
    "A faixa etária deve ser minor para menor de idade ou adult para maior de idade.")]
    [InlineData(
    "/api/transactions?minAmount=-1",
    "minAmount",
    "O valor mínimo não pode ser negativo.")]
    [InlineData(
    "/api/transactions?maxAmount=-1",
    "maxAmount",
    "O valor máximo não pode ser negativo.")]
    [InlineData(
    "/api/transactions?minAmount=10.999",
    "minAmount",
    "O valor mínimo deve possuir no máximo 2 casas decimais.")]
    [InlineData(
    "/api/transactions?maxAmount=10.999",
    "maxAmount",
    "O valor máximo deve possuir no máximo 2 casas decimais.")]
    public async Task ListTransactions_ShouldReturnBadRequest_WhenFilterIsInvalid(
    string requestUri,
    string propertyName,
    string expectedMessage)
    {
        using var response = await _client.GetAsync(requestUri);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        await using var responseStream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(responseStream);

        var root = document.RootElement;

        Assert.Equal(
            "Dados inválidos",
            root.GetProperty("title").GetString());

        Assert.Equal(
            400,
            root.GetProperty("status").GetInt32());

        var errors = root.GetProperty("errors");

        Assert.True(
            errors.TryGetProperty(
                propertyName,
                out var propertyErrors));

        Assert.Equal(
            expectedMessage,
            propertyErrors[0].GetString());
    }

    [Fact]
    public async Task ListTransactions_ShouldReturnBadRequest_WhenMinimumAmountExceedsMaximum()
    {
        using var response = await _client.GetAsync(
            "/api/transactions?minAmount=500&maxAmount=100");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        await using var responseStream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(responseStream);

        var errors = document.RootElement
            .GetProperty("errors");

        Assert.True(
            errors.TryGetProperty(
                "minAmount",
                out var minimumAmountErrors));

        Assert.Contains(
            minimumAmountErrors.EnumerateArray(),
            error =>
                error.GetString() ==
                "O valor mínimo não pode ser maior que o valor máximo.");
    }

    [Fact]
    public async Task UpdateTransaction_ShouldReturnBadRequest_WhenIdIsInvalid()
    {
        using var response =
            await _client.PutAsJsonAsync(
                "/api/transactions/0",
                new
                {
                    description = "Conta atualizada",
                    amount = 200m,
                    type = "expense",
                    personId = 1
                });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var errors = document.RootElement
            .GetProperty("errors");

        Assert.True(
            errors.TryGetProperty(
                "id",
                out var idErrors));

        Assert.Equal(
            "O identificador deve ser maior que zero.",
            idErrors[0].GetString());
    }

    [Fact]
    public async Task DeleteTransaction_ShouldReturnBadRequest_WhenIdIsInvalid()
    {
        using var response =
            await _client.DeleteAsync(
                "/api/transactions/0");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var idErrors = document.RootElement
            .GetProperty("errors")
            .GetProperty("id");

        Assert.Equal(
            "O identificador deve ser maior que zero.",
            idErrors[0].GetString());
    }

    [Fact]
    public async Task UpdateTransaction_ShouldReturnBadRequest_WhenBodyIsInvalid()
    {
        using var response =
            await _client.PutAsJsonAsync(
                "/api/transactions/1",
                new
                {
                    description = "",
                    amount = 0,
                    type = "expense",
                    personId = 0
                });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var errors = document.RootElement
            .GetProperty("errors");

        Assert.True(
            errors.TryGetProperty(
                "description",
                out _));

        Assert.True(
            errors.TryGetProperty(
                "amount",
                out _));

        Assert.True(
            errors.TryGetProperty(
                "personId",
                out _));
    }

    [Fact]
    public async Task UpdateTransaction_ShouldReturnBadRequest_WhenTypeIsNumeric()
    {
        using var response =
            await _client.PutAsJsonAsync(
                "/api/transactions/1",
                new
                {
                    description = "Conta atualizada",
                    amount = 200m,
                    type = 1,
                    personId = 1
                });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var errors = document.RootElement
            .GetProperty("errors");

        Assert.True(
            errors.TryGetProperty(
                "type",
                out var typeErrors));

        Assert.Equal(
            "O tipo deve ser 'expense' para despesa " +
            "ou 'income' para receita.",
            typeErrors[0].GetString());
    }
}