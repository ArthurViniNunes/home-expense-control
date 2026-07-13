using System.Text.Json;
using System.Text.Json.Serialization;
using HomeExpenseControl.Api.Domain.Enums;

namespace HomeExpenseControl.Api.Common.Serialization;

/// <summary>
/// Define a representação JSON do tipo da transação.
///
/// Os valores são escritos em camelCase e números não são aceitos,
/// mantendo o contrato da API explícito e legível.
/// </summary>
public sealed class TransactionTypeJsonConverter
    : JsonStringEnumConverter<TransactionType>
{
    public TransactionTypeJsonConverter()
        : base(
            JsonNamingPolicy.CamelCase,
            allowIntegerValues: false)
    {
    }
}