namespace HomeExpenseControl.Api.Domain.Entities;

/// <summary>
/// Representa uma pessoa responsável por receitas e despesas residenciais.
/// </summary>
public sealed class Person
{
    public const int MaxNameLength = 120;
    public const int MinNameLength = 2;
    public const int AdultAge = 18;


    // Construtor exigido pelo Entity Framework Core.
    private Person()
    {
    }

    public Person(string name, int age)
    {
        Name = ValidateAndNormalizeName(name);
        Age = ValidateAge(age);
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int Age { get; private set; }

    public bool IsMinor => Age < AdultAge;

    public ICollection<Transaction> Transactions { get; private set; } =
    new List<Transaction>();

    private static string ValidateAndNormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "O nome da pessoa é obrigatório.",
                nameof(name));
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length < MinNameLength)
        {
            throw new ArgumentException(
                $"O nome deve possuir no mínimo {MinNameLength} caracteres.",
                nameof(name));
        }

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"O nome deve possuir no máximo {MaxNameLength} caracteres.",
                nameof(name));
        }

        return normalizedName;
    }

    private static int ValidateAge(int age)
    {
        if (age < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(age),
                age,
                "A idade não pode ser negativa.");
        }

        return age;
    }
}