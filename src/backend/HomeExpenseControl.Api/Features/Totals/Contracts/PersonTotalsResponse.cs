namespace HomeExpenseControl.Api.Features.Totals.Contracts;

/// <summary>
/// Apresenta os totais financeiros de uma pessoa.
/// </summary>
/// <param name="PersonId">
/// Identificador da pessoa.
/// </param>
/// <param name="PersonName">
/// Nome da pessoa.
/// </param>
/// <param name="TotalIncome">
/// Soma das receitas da pessoa.
/// </param>
/// <param name="TotalExpenses">
/// Soma das despesas da pessoa.
/// </param>
/// <param name="Balance">
/// Saldo da pessoa, calculado como receitas menos despesas.
/// </param>
/// <example>
/// {
///   "personId": 1,
///   "personName": "Arthur Nunes",
///   "totalIncome": 3500.00,
///   "totalExpenses": 1250.50,
///   "balance": 2249.50
/// }
/// </example>
public sealed record PersonTotalsResponse(
    int PersonId,
    string PersonName,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance);