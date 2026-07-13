using System.Text.Json;
using HomeExpenseControl.Api.Domain.Enums;

namespace HomeExpenseControl.Api.Tests.Transactions;

public sealed class TransactionTypeSerializationTests
{
    [Fact]
    public void Serialize_ShouldReturnExpenseAsString()
    {
        var json = JsonSerializer.Serialize(TransactionType.Expense);

        Assert.Equal("\"expense\"", json);
    }

    [Fact]
    public void Serialize_ShouldReturnIncomeAsString()
    {
        var json = JsonSerializer.Serialize(TransactionType.Income);

        Assert.Equal("\"income\"", json);
    }

    [Theory]
    [InlineData("\"expense\"", TransactionType.Expense)]
    [InlineData("\"income\"", TransactionType.Income)]
    public void Deserialize_ShouldAcceptDocumentedStringValues(
        string json,
        TransactionType expectedType)
    {
        var result = JsonSerializer.Deserialize<TransactionType>(json);

        Assert.Equal(expectedType, result);
    }

    [Fact]
    public void Deserialize_ShouldRejectNumericValues()
    {
        Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<TransactionType>("1"));
    }
}