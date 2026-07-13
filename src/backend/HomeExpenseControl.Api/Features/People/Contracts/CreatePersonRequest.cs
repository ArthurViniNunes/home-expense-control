using System.ComponentModel.DataAnnotations;
using HomeExpenseControl.Api.Domain.Entities;

namespace HomeExpenseControl.Api.Features.People.Contracts;

/// <summary>
/// Dados necessários para cadastrar uma pessoa.
/// </summary>
/// <example>
/// {
///   "name": "Arthur Nunes",
///   "age": 22
/// }
/// </example>
public sealed class CreatePersonRequest
{
    /// <summary>
    /// Nome da pessoa.
    /// </summary>
    /// <example>Arthur Nunes</example>
    [Required(
        AllowEmptyStrings = false,
        ErrorMessage = "O nome é obrigatório.")]
    [StringLength(
        Person.MaxNameLength,
        ErrorMessage =
            "O nome deve possuir no máximo {1} caracteres.")]
    [MinLength(
        Person.MinNameLength,
        ErrorMessage = "O nome deve possuir no mínimo {1} caracteres.")]
    public required string Name { get; init; }

    /// <summary>
    /// Idade atual da pessoa, expressa em anos completos.
    /// </summary>
    /// <remarks>
    /// Pessoas com menos de 18 anos podem registrar somente despesas.
    /// </remarks>
    /// <example>22</example>
    [Range(
        0,
        int.MaxValue,
        ErrorMessage = "A idade não pode ser negativa.")]
    public required int Age { get; init; }
}