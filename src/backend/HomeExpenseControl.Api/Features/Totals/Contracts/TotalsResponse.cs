namespace HomeExpenseControl.Api.Features.Totals.Contracts;

/// <summary>
/// Reúne os totais individuais e o consolidado geral da residência.
/// </summary>
/// <param name="People">
/// Totais financeiros de cada pessoa cadastrada.
/// </param>
/// <param name="General">
/// Totais consolidados de todas as pessoas.
/// </param>
/// <example>
/// {
///   "people": [
///     {
///       "personId": 1,
///       "personName": "Arthur Nunes",
///       "totalIncome": 3500.00,
///       "totalExpenses": 1250.50,
///       "balance": 2249.50
///     }
///   ],
///   "general": {
///     "totalIncome": 3500.00,
///     "totalExpenses": 1250.50,
///     "netBalance": 2249.50
///   }
/// }
/// </example>
public sealed record TotalsResponse(
    IReadOnlyList<PersonTotalsResponse> People,
    GeneralTotalsResponse General);