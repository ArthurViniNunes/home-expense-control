namespace HomeExpenseControl.Api.Features.Transactions.Contracts;

/// <summary>
/// Identifica a pessoa vinculada à transação.
/// </summary>
/// <param name="Id">Identificador da pessoa.</param>
/// <param name="Name">Nome da pessoa.</param>
public sealed record TransactionPersonResponse(
    int Id,
    string Name);