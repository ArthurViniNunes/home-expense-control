using System.ComponentModel.DataAnnotations;
using HomeExpenseControl.Api.Common.Money;
using HomeExpenseControl.Api.Domain.Enums;

namespace HomeExpenseControl.Api.Features.Transactions.Contracts;

/// <summary>
/// Parâmetros opcionais utilizados para filtrar transações.
/// </summary>
public sealed class ListTransactionsQuery : IValidatableObject
{
    /// <summary>
    /// Identificador da pessoa vinculada à transação.
    /// </summary>
    /// <example>1</example>
    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "O identificador da pessoa deve ser maior que zero.")]
    public int? PersonId { get; init; }

    /// <summary>
    /// Faixa etária da pessoa vinculada à transação.
    /// </summary>
    /// <remarks>
    /// Use minor para menores de 18 anos ou adult para maiores de idade.
    /// </remarks>
    /// <example>adult</example>
    [RegularExpression(
        "^(minor|adult)$",
        ErrorMessage =
            "A faixa etária deve ser minor para menor de idade " +
            "ou adult para maior de idade.")]
    public string? AgeGroup { get; init; }

    /// <summary>
    /// Tipo da transação.
    /// </summary>
    /// <remarks>
    /// Use expense para despesas ou income para receitas.
    /// </remarks>
    /// <example>expense</example>
    [RegularExpression(
        "^(expense|income)$",
        ErrorMessage =
            "O tipo deve ser expense para despesa ou income para receita.")]
    public string? Type { get; init; }

    /// <summary>
    /// Menor valor aceito no resultado.
    /// </summary>
    /// <example>100.00</example>
    [Range(
        typeof(decimal),
        "0",
        "92233720368547758.07",
        ErrorMessage =
            "O valor mínimo não pode ser negativo.")]
    public decimal? MinAmount { get; init; }

    /// <summary>
    /// Maior valor aceito no resultado.
    /// </summary>
    /// <example>500.00</example>
    [Range(
        typeof(decimal),
        "0",
        "92233720368547758.07",
        ErrorMessage =
            "O valor máximo não pode ser negativo.")]
    public decimal? MaxAmount { get; init; }

    /// <summary>
    /// Converte o tipo textual validado para o enum interno.
    /// </summary>
    public TransactionType? GetTransactionType()
    {
        return Type switch
        {
            "expense" => TransactionType.Expense,
            "income" => TransactionType.Income,
            _ => null
        };
    }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (HasMoreThanTwoDecimalPlaces(MinAmount))
        {
            yield return new ValidationResult(
                $"O valor mínimo deve possuir no máximo " +
                $"{MoneyConverter.DecimalPlaces} casas decimais.",
                [nameof(MinAmount)]);
        }

        if (HasMoreThanTwoDecimalPlaces(MaxAmount))
        {
            yield return new ValidationResult(
                $"O valor máximo deve possuir no máximo " +
                $"{MoneyConverter.DecimalPlaces} casas decimais.",
                [nameof(MaxAmount)]);
        }

        if (
            MinAmount.HasValue &&
            MaxAmount.HasValue &&
            MinAmount.Value > MaxAmount.Value)
        {
            yield return new ValidationResult(
                "O valor mínimo não pode ser maior que o valor máximo.",
                [
                    nameof(MinAmount),
                    nameof(MaxAmount)
                ]);
        }
    }

    private static bool HasMoreThanTwoDecimalPlaces(
        decimal? amount)
    {
        return amount.HasValue &&
            decimal.Round(
                amount.Value,
                MoneyConverter.DecimalPlaces) != amount.Value;
    }
}