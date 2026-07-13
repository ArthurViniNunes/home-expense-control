using System.ComponentModel.DataAnnotations;
using HomeExpenseControl.Api.Domain.Entities;
using HomeExpenseControl.Api.Domain.Enums;

namespace HomeExpenseControl.Api.Features.Transactions.Contracts;

/// <summary>
/// Dados necessários para cadastrar uma transação.
/// </summary>
/// <example>
/// {
///   "description": "Conta de energia",
///   "amount": 125.90,
///   "type": "expense",
///   "personId": 1
/// }
/// </example>
public sealed class CreateTransactionRequest
{
    /// <summary>
    /// Descrição que permite identificar a movimentação financeira.
    /// </summary>
    /// <remarks>
    /// Informe uma descrição objetiva, como Conta de energia,
    /// Salário ou Compra de supermercado.
    /// </remarks>
    /// <example>Conta de energia</example>
    [Required(
        AllowEmptyStrings = false,
        ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(
        Transaction.MaxDescriptionLength,
        ErrorMessage =
            "A descrição deve possuir no máximo {1} caracteres.")]
    public required string Description { get; init; }

    /// <summary>
    /// Valor monetário da transação, maior que zero.
    /// </summary>
    /// <remarks>
    /// O valor deve possuir no máximo duas casas decimais.
    /// </remarks>
    /// <example>125.90</example>
    [Range(
        typeof(decimal),
        "0.01",
        "79228162514264337593543950335",
        ErrorMessage = "O valor deve ser maior que zero.")]
    public required decimal Amount { get; init; }

    /// <summary>
    /// Classificação da transação.
    /// </summary>
    /// <remarks>
    /// Use expense para despesa ou income para receita.
    /// Pessoas menores de 18 anos podem registrar somente despesas.
    /// </remarks>
    /// <example>expense</example>
    [EnumDataType(
        typeof(TransactionType),
        ErrorMessage =
            "O tipo deve ser expense para despesa ou income para receita.")]
    public required TransactionType Type { get; init; }

    /// <summary>
    /// Identificador da pessoa responsável pela transação.
    /// </summary>
    /// <remarks>
    /// A pessoa deve estar previamente cadastrada no sistema.
    /// </remarks>
    /// <example>3</example>
    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "O identificador da pessoa deve ser maior que zero.")]
    public required int PersonId { get; init; }
}