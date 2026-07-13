using System.Text.Json.Serialization;
using HomeExpenseControl.Api.Common.Serialization;

namespace HomeExpenseControl.Api.Domain.Enums;

/// <summary>
/// Define os tipos de movimentação financeira aceitos pela aplicação.
/// </summary>
[JsonConverter(typeof(TransactionTypeJsonConverter))]
public enum TransactionType
{
    /// <summary>
    /// Representa uma despesa.
    /// </summary>
    [JsonStringEnumMemberName("expense")]
    Expense = 1,

    /// <summary>
    /// Representa uma receita.
    /// </summary>
    [JsonStringEnumMemberName("income")]
    Income = 2
}