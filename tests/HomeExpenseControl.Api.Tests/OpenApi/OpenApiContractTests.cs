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
                builder.UseEnvironment("Testing");
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

    [Fact]
    public async Task OpenApi_ShouldDocumentUpdateTransactionEndpoint()
    {
        using var response = await _client.GetAsync(
            "/openapi/v1.json");

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var updateOperation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/transactions/{id}")
            .GetProperty("put");

        Assert.Equal(
            "UpdateTransaction",
            updateOperation
                .GetProperty("operationId")
                .GetString());

        var responses =
            updateOperation.GetProperty("responses");

        Assert.True(
            responses.TryGetProperty(
                "200",
                out _));

        Assert.True(
            responses.TryGetProperty(
                "400",
                out _));

        Assert.True(
            responses.TryGetProperty(
                "404",
                out _));

        Assert.True(
            responses.TryGetProperty(
                "422",
                out _));

        var content = updateOperation
            .GetProperty("requestBody")
            .GetProperty("content");

        Assert.True(
            content.TryGetProperty(
                "application/json",
                out _));
    }

    [Fact]
    public async Task OpenApi_ShouldDocumentDeleteTransactionEndpoint()
    {
        using var response = await _client.GetAsync(
            "/openapi/v1.json");

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var deleteOperation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/transactions/{id}")
            .GetProperty("delete");

        Assert.Equal(
            "DeleteTransaction",
            deleteOperation
                .GetProperty("operationId")
                .GetString());

        var responses =
            deleteOperation.GetProperty("responses");

        Assert.True(
            responses.TryGetProperty(
                "204",
                out _));

        Assert.True(
            responses.TryGetProperty(
                "400",
                out _));

        Assert.True(
            responses.TryGetProperty(
                "404",
                out _));
    }

    [Fact]
    public async Task OpenApi_ShouldDocumentUpdateTransactionRequestSchema()
    {
        using var response = await _client.GetAsync(
            "/openapi/v1.json");

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var schema = document.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("UpdateTransactionRequest");

        var requiredProperties = schema
            .GetProperty("required")
            .EnumerateArray()
            .Select(property => property.GetString())
            .ToHashSet();

        Assert.Contains(
            "description",
            requiredProperties);

        Assert.Contains(
            "amount",
            requiredProperties);

        Assert.Contains(
            "type",
            requiredProperties);

        Assert.Contains(
            "personId",
            requiredProperties);

        var properties =
            schema.GetProperty("properties");

        Assert.Equal(
            "string",
            properties
                .GetProperty("description")
                .GetProperty("type")
                .GetString());

        Assert.Equal(
            "number",
            properties
                .GetProperty("amount")
                .GetProperty("type")
                .GetString());

        Assert.Equal(
            "integer",
            properties
                .GetProperty("personId")
                .GetProperty("type")
                .GetString());

        Assert.Equal(
            "int32",
            properties
                .GetProperty("personId")
                .GetProperty("format")
                .GetString());

        var transactionTypeReference = properties
            .GetProperty("type")
            .GetProperty("$ref")
            .GetString();

        Assert.Equal(
            "#/components/schemas/TransactionType",
            transactionTypeReference);
    }

    [Fact]
    public async Task OpenApi_ShouldOrderTagsByBusinessFlow()
    {
        using var response = await _client.GetAsync(
            "/openapi/v1.json");

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var tags = document.RootElement
            .GetProperty("tags")
            .EnumerateArray()
            .Select(tag =>
                tag.GetProperty("name").GetString()!)
            .ToArray();

        Assert.Equal(
            [
                "Pessoas",
                "Transações",
                "Totais"
            ],
            tags);
    }
}