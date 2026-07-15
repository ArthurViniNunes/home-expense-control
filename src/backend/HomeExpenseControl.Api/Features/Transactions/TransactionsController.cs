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
    /// Lista as transações cadastradas com filtros opcionais.
    /// </summary>
    /// <remarks>
    /// Os filtros são opcionais e podem ser combinados.
    ///
    /// Filtros disponíveis:
    ///
    /// - personId: identificador da pessoa;
    /// - ageGroup: adult para maiores de idade ou minor para menores;
    /// - type: expense para despesas ou income para receitas;
    /// - minAmount: valor mínimo, inclusive;
    /// - maxAmount: valor máximo, inclusive.
    ///
    /// Exemplos:
    ///
    /// GET /api/transactions?personId=1
    ///
    /// GET /api/transactions?ageGroup=minor
    ///
    /// GET /api/transactions?type=expense
    ///
    /// GET /api/transactions?minAmount=100&amp;maxAmount=500
    ///
    /// GET /api/transactions?personId=1&amp;ageGroup=adult&amp;type=expense&amp;minAmount=100&amp;maxAmount=500
    ///
    /// Quando nenhum filtro é informado, todas as transações são retornadas.
    ///
    /// As transações são apresentadas na ordem de seus identificadores.
    /// </remarks>
    /// <param name="query">Filtros opcionais da consulta.</param>
    /// <param name="cancellationToken">
    /// Token utilizado para cancelar a operação.
    /// </param>
    /// <returns>Lista de transações que atendem aos filtros.</returns>
    /// <response code="200">Transações consultadas com sucesso.</response>
    /// <response code="400">Um ou mais filtros são inválidos.</response>
    [HttpGet]
    [EndpointName("ListTransactions")]
    [ProducesResponseType(
        typeof(IReadOnlyList<TransactionResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> List(
        [FromQuery] ListTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        var transactions = await _transactionsService.ListAsync(
            query,
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
        [FromRoute]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "O identificador deve ser maior que zero.")]
        int id,
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