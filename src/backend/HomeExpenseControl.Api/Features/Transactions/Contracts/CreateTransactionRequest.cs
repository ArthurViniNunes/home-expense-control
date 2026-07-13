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
    /// Descrição da receita ou despesa.
    /// </summary>
    /// <example>Conta de energia</example>
    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(
        Transaction.MaxDescriptionLength,
        ErrorMessage = "A descrição deve possuir no máximo {1} caracteres.")]
    public string? Description { get; init; }

    /// <summary>
    /// Valor monetário da transação.
    /// </summary>
    /// <remarks>
    /// O valor deve ser maior que zero e possuir no máximo duas casas decimais.
    /// </remarks>
    /// <example>125.90</example>
    [Required(ErrorMessage = "O valor é obrigatório.")]
    public decimal? Amount { get; init; }

    /// <summary>
    /// Tipo da transação: expense para despesa ou income para receita.
    /// </summary>
    /// <example>expense</example>
    [Required(ErrorMessage = "O tipo da transação é obrigatório.")]
    [EnumDataType(
        typeof(TransactionType),
        ErrorMessage = "O tipo da transação deve ser despesa ou receita.")]
    public TransactionType? Type { get; init; }

    /// <summary>
    /// Identificador da pessoa responsável pela transação.
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "A pessoa é obrigatória.")]
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "O identificador da pessoa deve ser maior que zero.")]
    public int? PersonId { get; init; }
}