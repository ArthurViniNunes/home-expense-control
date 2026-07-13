using System.ComponentModel.DataAnnotations;
using HomeExpenseControl.Api.Features.Transactions.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HomeExpenseControl.Api.Features.Transactions;

/// <summary>
/// Disponibiliza operações para cadastro e consulta de transações.
/// </summary>
[ApiController]
[Route("api/transactions")]
[Tags("Transações")]
public sealed class TransactionsController : ControllerBase
{
    private readonly TransactionsService _transactionsService;

    public TransactionsController(TransactionsService transactionsService)
    {
        _transactionsService = transactionsService;
    }

    /// <summary>
    /// Cadastra uma receita ou despesa.
    /// </summary>
    /// <remarks>
    /// A pessoa informada deve existir.
    ///
    /// Pessoas menores de 18 anos podem cadastrar somente despesas.
    ///
    /// O valor deve ser maior que zero e possuir no máximo duas casas decimais.
    /// </remarks>
    /// <param name="request">Dados da transação.</param>
    /// <param name="cancellationToken">
    /// Token utilizado para cancelar a operação.
    /// </param>
    /// <returns>A transação cadastrada.</returns>
    /// <response code="201">Transação cadastrada com sucesso.</response>
    /// <response code="400">Os dados informados são inválidos.</response>
    /// <response code="404">A pessoa informada não existe.</response>
    /// <response code="422">
    /// A transação viola uma regra de negócio, como uma receita para menor.
    /// </response>
    [HttpPost]
    [EndpointName("CreateTransaction")]
    [ProducesResponseType(
        typeof(TransactionResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TransactionResponse>> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionsService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = transaction.Id },
            transaction);
    }

    /// <summary>
    /// Lista todas as transações cadastradas.
    /// </summary>
    /// <remarks>
    /// As transações são apresentadas na ordem de seus identificadores.
    /// </remarks>
    /// <param name="cancellationToken">
    /// Token utilizado para cancelar a operação.
    /// </param>
    /// <returns>Lista de transações cadastradas.</returns>
    /// <response code="200">Transações consultadas com sucesso.</response>
    [HttpGet]
    [EndpointName("ListTransactions")]
    [ProducesResponseType(
        typeof(IReadOnlyList<TransactionResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> List(
        CancellationToken cancellationToken)
    {
        var transactions = await _transactionsService.ListAsync(
            cancellationToken);

        return Ok(transactions);
    }

    /// <summary>
    /// Consulta uma transação pelo identificador.
    /// </summary>
    /// <param name="id" example="1">
    /// Identificador da transação.
    /// </param>
    /// <param name="cancellationToken">
    /// Token utilizado para cancelar a operação.
    /// </param>
    /// <returns>A transação encontrada.</returns>
    /// <response code="200">Transação encontrada.</response>
    /// <response code="400">O identificador é inválido.</response>
    /// <response code="404">A transação não foi encontrada.</response>
    [HttpGet("{id:int}")]
    [EndpointName("GetTransactionById")]
    [ProducesResponseType(
        typeof(TransactionResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetById(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionsService.GetByIdAsync(
            id,
            cancellationToken);

        if (transaction is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Transação não encontrada",
                Detail =
                    $"Não existe uma transação cadastrada com o identificador {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(transaction);
    }
}