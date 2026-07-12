namespace HomeExpenseControl.Api.Common.Money;

/// <summary>
/// Centraliza a conversão entre valores decimais e centavos inteiros.
/// </summary>
public static class MoneyConverter
{
    public const int DecimalPlaces = 2;

    private const decimal CentsFactor = 100m;

    public static long ToCents(decimal amount)
    {
        if (decimal.Round(amount, DecimalPlaces) != amount)
        {
            throw new ArgumentException(
                $"O valor monetário deve possuir no máximo {DecimalPlaces} casas decimais.",
                nameof(amount));
        }

        return checked((long)(amount * CentsFactor));
    }

    public static decimal FromCents(long amountInCents)
    {
        return amountInCents / CentsFactor;
    }
}