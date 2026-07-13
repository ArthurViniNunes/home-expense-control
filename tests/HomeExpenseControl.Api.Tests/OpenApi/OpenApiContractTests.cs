using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace HomeExpenseControl.Api.Tests.OpenApi;

public sealed class OpenApiContractTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OpenApiContractTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            })
            .CreateClient();
    }

    [Fact]
    public async Task OpenApi_ShouldRepresentPersonIdOnlyAsInteger()
    {
        using var response = await _client.GetAsync(
            "/openapi/v1.json");

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document = await JsonDocument.ParseAsync(stream);

        var personIdSchema = document.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("CreateTransactionRequest")
            .GetProperty("properties")
            .GetProperty("personId");

        Assert.Equal(
            JsonValueKind.String,
            personIdSchema.GetProperty("type").ValueKind);

        Assert.Equal(
            "integer",
            personIdSchema.GetProperty("type").GetString());

        Assert.Equal(
            "int32",
            personIdSchema.GetProperty("format").GetString());

        Assert.False(
            personIdSchema.TryGetProperty(
                "pattern",
                out _));
    }

    [Fact]
    public async Task OpenApi_ShouldDocumentTransactionTypeAsStringEnum()
    {
        using var response = await _client.GetAsync(
            "/openapi/v1.json");

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document = await JsonDocument.ParseAsync(stream);

        var transactionTypeSchema = document.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("TransactionType");

        Assert.Equal(
            "string",
            transactionTypeSchema.GetProperty("type").GetString());

        var allowedValues = transactionTypeSchema
            .GetProperty("enum")
            .EnumerateArray()
            .Select(value => value.GetString()!)
            .ToArray();

        Assert.Equal(
            ["expense", "income"],
            allowedValues);
    }

    [Fact]
    public async Task OpenApi_ShouldDocumentTotalsEndpoint()
    {
        using var response = await _client.GetAsync(
            "/openapi/v1.json");

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var totalsOperation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/totals")
            .GetProperty("get");

        Assert.Equal(
            "GetTotals",
            totalsOperation
                .GetProperty("operationId")
                .GetString());

        Assert.True(
            totalsOperation
                .GetProperty("responses")
                .TryGetProperty("200", out _));
    }
}