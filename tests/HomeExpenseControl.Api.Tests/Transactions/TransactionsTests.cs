using HomeExpenseControl.Api.Common.Errors;
using HomeExpenseControl.Api.Domain.Entities;
using HomeExpenseControl.Api.Domain.Enums;

namespace HomeExpenseControl.Api.Tests.Transactions;

public sealed class TransactionTests
{
    [Fact]
    public void Constructor_ShouldCreateExpense_WhenDataIsValid()
    {
        var person = new Person("Arthur Nunes", 22);

        var transaction = new Transaction(
            "Conta de energia",
            125.90m,
            TransactionType.Expense,
            person);

        Assert.Equal("Conta de energia", transaction.Description);
        Assert.Equal(12590, transaction.AmountInCents);
        Assert.Equal(125.90m, transaction.Amount);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Same(person, transaction.Person);
    }

    [Fact]
    public void Constructor_ShouldCreateIncome_WhenPersonIsAdult()
    {
        var person = new Person("Carlos Souza", 18);

        var transaction = new Transaction(
            "Salário",
            2500m,
            TransactionType.Income,
            person);

        Assert.Equal(TransactionType.Income, transaction.Type);
        Assert.Equal(250000, transaction.AmountInCents);
    }

    [Fact]
    public void Constructor_ShouldAllowExpense_WhenPersonIsMinor()
    {
        var person = new Person("Pedro Souza", 17);

        var transaction = new Transaction(
            "Material escolar",
            85.50m,
            TransactionType.Expense,
            person);

        Assert.Equal(TransactionType.Expense, transaction.Type);
    }

    [Fact]
    public void Constructor_ShouldThrowBusinessRuleException_WhenMinorRegistersIncome()
    {
        var person = new Person("Pedro Souza", 17);

        var exception = Assert.Throws<BusinessRuleException>(
            () => new Transaction(
                "Mesada",
                100m,
                TransactionType.Income,
                person));

        Assert.Contains(
            "menores de 18 anos",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowArgumentException_WhenDescriptionIsInvalid(
        string? description)
    {
        var person = new Person("Arthur Nunes", 22);

        Assert.Throws<ArgumentException>(
            () => new Transaction(
                description!,
                100m,
                TransactionType.Expense,
                person));
    }

    [Fact]
    public void Constructor_ShouldNormalizeDescription()
    {
        var person = new Person("Arthur Nunes", 22);

        var transaction = new Transaction(
            "  Conta de água  ",
            100m,
            TransactionType.Expense,
            person);

        Assert.Equal("Conta de água", transaction.Description);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenDescriptionExceedsLimit()
    {
        var person = new Person("Arthur Nunes", 22);
        var description = new string(
            'A',
            Transaction.MaxDescriptionLength + 1);

        Assert.Throws<ArgumentException>(
            () => new Transaction(
                description,
                100m,
                TransactionType.Expense,
                person));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenAmountIsNotPositive(
        decimal amount)
    {
        var person = new Person("Arthur Nunes", 22);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Transaction(
                "Compra",
                amount,
                TransactionType.Expense,
                person));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenAmountHasMoreThanTwoDecimalPlaces()
    {
        var person = new Person("Arthur Nunes", 22);

        var exception = Assert.Throws<ArgumentException>(
            () => new Transaction(
                "Compra",
                10.999m,
                TransactionType.Expense,
                person));

        Assert.Equal("amount", exception.ParamName);

        Assert.Contains(
            "no máximo 2 casas decimais",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenTypeIsInvalid()
    {
        var person = new Person("Arthur Nunes", 22);
        var invalidType = (TransactionType)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Transaction(
                "Compra",
                100m,
                invalidType,
                person));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenPersonIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new Transaction(
                "Compra",
                100m,
                TransactionType.Expense,
                null!));
    }

    [Fact]
    public void Update_ShouldReplaceTransactionData_WhenValuesAreValid()
    {
        var originalPerson = new Person(
            "Carlos Souza",
            35);

        var newPerson = new Person(
            "Maria Souza",
            28);

        var transaction = new Transaction(
            "Conta de energia",
            125.90m,
            TransactionType.Expense,
            originalPerson);

        transaction.Update(
            "Salário mensal",
            3500m,
            TransactionType.Income,
            newPerson);

        Assert.Equal(
            "Salário mensal",
            transaction.Description);

        Assert.Equal(
            3500m,
            transaction.Amount);

        Assert.Equal(
            TransactionType.Income,
            transaction.Type);

        Assert.Same(
            newPerson,
            transaction.Person);
    }

    [Fact]
    public void Update_ShouldNormalizeDescription()
    {
        var person = new Person(
            "Carlos Souza",
            35);

        var transaction = new Transaction(
            "Conta de energia",
            125.90m,
            TransactionType.Expense,
            person);

        transaction.Update(
            "  Conta de internet  ",
            150m,
            TransactionType.Expense,
            person);

        Assert.Equal(
            "Conta de internet",
            transaction.Description);
    }

    [Fact]
    public void Update_ShouldKeepOriginalState_WhenMinorIncomeIsRejected()
    {
        var adult = new Person(
            "Carlos Souza",
            35);

        var minor = new Person(
            "Pedro Souza",
            17);

        var transaction = new Transaction(
            "Conta de energia",
            125.90m,
            TransactionType.Expense,
            adult);

        Assert.Throws<BusinessRuleException>(
            () => transaction.Update(
                "Mesada",
                500m,
                TransactionType.Income,
                minor));

        Assert.Equal(
            "Conta de energia",
            transaction.Description);

        Assert.Equal(
            125.90m,
            transaction.Amount);

        Assert.Equal(
            TransactionType.Expense,
            transaction.Type);

        Assert.Same(
            adult,
            transaction.Person);
    }

    [Fact]
    public void Update_ShouldKeepOriginalState_WhenAmountHasMoreThanTwoDecimalPlaces()
    {
        var person = new Person(
            "Carlos Souza",
            35);

        var transaction = new Transaction(
            "Conta de energia",
            125.90m,
            TransactionType.Expense,
            person);

        Assert.Throws<ArgumentException>(
            () => transaction.Update(
                "Conta atualizada",
                150.999m,
                TransactionType.Expense,
                person));

        Assert.Equal(
            "Conta de energia",
            transaction.Description);

        Assert.Equal(
            125.90m,
            transaction.Amount);

        Assert.Equal(
            TransactionType.Expense,
            transaction.Type);

        Assert.Same(
            person,
            transaction.Person);
    }
}