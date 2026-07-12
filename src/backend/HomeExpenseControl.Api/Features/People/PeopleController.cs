using System.ComponentModel.DataAnnotations;
using HomeExpenseControl.Api.Features.People.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HomeExpenseControl.Api.Features.People;

/// <summary>
/// Disponibiliza operações para o gerenciamento de pessoas.
/// </summary>
[ApiController]
[Route("api/people")]
[Tags("Pessoas")]
public sealed class PeopleController : ControllerBase
{
    private readonly PeopleService _peopleService;

    public PeopleController(PeopleService peopleService)
    {
        _peopleService = peopleService;
    }

    /// <summary>
    /// Cadastra uma nova pessoa.
    /// </summary>
    /// <remarks>
    /// O identificador é gerado automaticamente.
    ///
    /// O nome é normalizado antes do armazenamento, removendo espaços
    /// existentes no início e no final.
    /// </remarks>
    /// <param name="request">Dados da pessoa que será cadastrada.</param>
    /// <param name="cancellationToken">
    /// Token utilizado para cancelar a operação.
    /// </param>
    /// <returns>A pessoa cadastrada.</returns>
    /// <response code="201">
    /// Pessoa cadastrada com sucesso.
    /// </response>
    /// <response code="400">
    /// Os dados informados são inválidos.
    /// </response>
    [HttpPost]
    [EndpointName("CreatePerson")]
    [ProducesResponseType(
        typeof(PersonResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PersonResponse>> Create(
        [FromBody] CreatePersonRequest request,
        CancellationToken cancellationToken)
    {
        var person = await _peopleService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = person.Id },
            person);
    }

    /// <summary>
    /// Lista todas as pessoas cadastradas.
    /// </summary>
    /// <remarks>
    /// As pessoas são ordenadas pelo nome e, em caso de nomes iguais,
    /// pelo identificador.
    /// </remarks>
    /// <param name="cancellationToken">
    /// Token utilizado para cancelar a operação.
    /// </param>
    /// <returns>Lista de pessoas cadastradas.</returns>
    /// <response code="200">
    /// Pessoas consultadas com sucesso.
    /// </response>
    [HttpGet]
    [EndpointName("ListPeople")]
    [ProducesResponseType(
        typeof(IReadOnlyList<PersonResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersonResponse>>> List(
        CancellationToken cancellationToken)
    {
        var people = await _peopleService.ListAsync(
            cancellationToken);

        return Ok(people);
    }

    /// <summary>
    /// Consulta uma pessoa pelo identificador.
    /// </summary>
    /// <param name="id" example="1">
    /// Identificador da pessoa.
    /// </param>
    /// <param name="cancellationToken">
    /// Token utilizado para cancelar a operação.
    /// </param>
    /// <returns>A pessoa encontrada.</returns>
    /// <response code="200">
    /// Pessoa encontrada.
    /// </response>
    /// <response code="400">
    /// O identificador informado é inválido.
    /// </response>
    /// <response code="404">
    /// Não existe uma pessoa com o identificador informado.
    /// </response>
    [HttpGet("{id:int}")]
    [EndpointName("GetPersonById")]
    [ProducesResponseType(
        typeof(PersonResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonResponse>> GetById(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var person = await _peopleService.GetByIdAsync(
            id,
            cancellationToken);

        if (person is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Pessoa não encontrada",
                Detail =
                    $"Não existe uma pessoa cadastrada com o identificador {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(person);
    }
}