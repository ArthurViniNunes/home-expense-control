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
    /// Nome completo da pessoa.
    /// </summary>
    /// <example>Arthur Nunes</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(
        Person.MaxNameLength,
        ErrorMessage = "O nome deve possuir no máximo {1} caracteres.")]
    public string? Name { get; init; }

    /// <summary>
    /// Idade atual da pessoa.
    /// </summary>
    /// <example>22</example>
    [Required(ErrorMessage = "A idade é obrigatória.")]
    [Range(
        0,
        int.MaxValue,
        ErrorMessage = "A idade não pode ser negativa.")]
    public int? Age { get; init; }
}