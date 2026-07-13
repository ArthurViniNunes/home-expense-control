namespace HomeExpenseControl.Api.Features.Totals.Contracts;

/// <summary>
/// Apresenta os totais financeiros gerais da residência.
/// </summary>
/// <param name="TotalIncome">
/// Soma das receitas de todas as pessoas.
/// </param>
/// <param name="TotalExpenses">
/// Soma das despesas de todas as pessoas.
/// </param>
/// <param name="NetBalance">
/// Saldo líquido geral, calculado como receitas menos despesas.
/// </param>
/// <example>
/// {
///   "totalIncome": 6500.00,
///   "totalExpenses": 3000.50,
///   "netBalance": 3499.50
/// }
/// </example>
public sealed record GeneralTotalsResponse(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetBalance);