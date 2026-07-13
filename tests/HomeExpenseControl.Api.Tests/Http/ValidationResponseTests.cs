using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

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
}