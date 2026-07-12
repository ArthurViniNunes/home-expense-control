namespace HomeExpenseControl.Api.Features.People.Contracts;

/// <summary>
/// Representa uma pessoa cadastrada no sistema.
/// </summary>
/// <param name="Id">Identificador único da pessoa.</param>
/// <param name="Name">Nome da pessoa.</param>
/// <param name="Age">Idade da pessoa.</param>
/// <param name="IsMinor">
/// Indica se a pessoa possui menos de 18 anos.
/// </param>
/// <example>
/// {
///   "id": 1,
///   "name": "Arthur Nunes",
///   "age": 22,
///   "isMinor": false
/// }
/// </example>
public sealed record PersonResponse(
    int Id,
    string Name,
    int Age,
    bool IsMinor);