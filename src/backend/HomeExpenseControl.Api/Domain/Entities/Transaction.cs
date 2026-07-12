using HomeExpenseControl.Api.Common.Errors;
using HomeExpenseControl.Api.Common.Money;
using HomeExpenseControl.Api.Domain.Enums;

namespace HomeExpenseControl.Api.Domain.Entities;

/// <summary>
/// Representa uma receita ou despesa vinculada a uma pessoa.
/// </summary>
public sealed class Transaction
{
    public const int MaxDescriptionLength = 200;

    // Construtor utilizado pelo Entity Framework Core.
    private Transaction()
    {
    }

    public Transaction(
        string description,
        decimal amount,
        TransactionType type,
        Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        Description = ValidateAndNormalizeDescription(description);
        AmountInCents = ValidateAndConvertAmount(amount);
        Type = ValidateType(type);

        ValidateMinorRestriction(person, Type);

        Person = person;
        PersonId = person.Id;
    }

    public int Id { get; private set; }

    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Valor armazenado em centavos para evitar perda de precisão.
    /// </summary>
    public long AmountInCents { get; private set; }

    /// <summary>
    /// Representação decimal utilizada pela aplicação e pela API.
    /// </summary>
    public decimal Amount => MoneyConverter.FromCents(AmountInCents);

    public TransactionType Type { get; private set; }

    public int PersonId { get; private set; }

    public Person Person { get; private set; } = null!;

    private static string ValidateAndNormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "A descrição da transação é obrigatória.",
                nameof(description));
        }

        var normalizedDescription = description.Trim();

        if (normalizedDescription.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"A descrição deve possuir no máximo {MaxDescriptionLength} caracteres.",
                nameof(description));
        }

        return normalizedDescription;
    }

    private static long ValidateAndConvertAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "O valor da transação deve ser maior que zero.");
        }

        return MoneyConverter.ToCents(amount);
    }

    private static TransactionType ValidateType(TransactionType type)
    {
        if (!Enum.IsDefined(typeof(TransactionType), type))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "O tipo da transação deve ser despesa ou receita.");
        }

        return type;
    }

    private static void ValidateMinorRestriction(
        Person person,
        TransactionType type)
    {
        if (person.IsMinor && type == TransactionType.Income)
        {
            throw new BusinessRuleException(
                "Pessoas menores de 18 anos não podem cadastrar receitas.");
        }
    }
}