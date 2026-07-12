using HomeExpenseControl.Api.Domain.Entities;

namespace HomeExpenseControl.Api.Tests.People;

public sealed class PeopleTests
{
    // Testes seguem formato: Método_ResultadoEsperado_Cenário

    [Fact]
    public void Constructor_ShouldCreatePerson_WhenDataIsValid()
    {
        var person = new Person("Arthur Nunes", 22);

        Assert.Equal("Arthur Nunes", person.Name);
        Assert.Equal(22, person.Age);
        Assert.False(person.IsMinor);
    }

    [Fact]
    public void Constructor_ShouldRemoveOuterSpaces_FromName()
    {
        var person = new Person("  Exemplo da Silva  ", 25);

        Assert.Equal("Exemplo da Silva", person.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(
        string? invalidName)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new Person(invalidName!, 20));

        Assert.Contains(
            "nome da pessoa é obrigatório",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenNameExceedsLimit()
    {
        var invalidName = new string('A', Person.MaxNameLength + 1);

        var exception = Assert.Throws<ArgumentException>(
            () => new Person(invalidName, 20));

        Assert.Contains(
            Person.MaxNameLength.ToString(),
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenAgeIsNegative()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Person("Arthur Nunes", -1));

        Assert.Equal("age", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(17)]
    public void IsMinor_ShouldReturnTrue_WhenAgeIsLowerThanEighteen(int age)
    {
        var person = new Person("Pessoa menor", age);

        Assert.True(person.IsMinor);
    }

    [Theory]
    [InlineData(18)]
    [InlineData(22)]
    [InlineData(70)]
    public void IsMinor_ShouldReturnFalse_WhenAgeIsAtLeastEighteen(int age)
    {
        var person = new Person("Pessoa adulta", age);

        Assert.False(person.IsMinor);
    }
}