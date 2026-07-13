using HomeExpenseControl.Api.Domain.Enums;

namespace HomeExpenseControl.Api.Features.Transactions.Contracts;

/// <summary>
/// Representa uma transação cadastrada.
/// </summary>
/// <param name="Id">Identificador da transação.</param>
/// <param name="Description">Descrição da transação.</param>
/// <param name="Amount">Valor monetário da transação.</param>
/// <param name="Type">Tipo da transação.</param>
/// <param name="Person">Pessoa vinculada à transação.</param>
/// <example>
/// {
///   "id": 1,
///   "description": "Conta de energia",
///   "amount": 125.90,
///   "type": "expense",
///   "person": {
///     "id": 1,
///     "name": "Arthur Nunes"
///   }
/// }
/// </example>
public sealed record TransactionResponse(
    int Id,
    string Description,
    decimal Amount,
    TransactionType Type,
    TransactionPersonResponse Person);