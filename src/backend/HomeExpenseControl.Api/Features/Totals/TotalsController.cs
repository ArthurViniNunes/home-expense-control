using HomeExpenseControl.Api.Features.Totals.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HomeExpenseControl.Api.Features.Totals;

/// <summary>
/// Disponibiliza a consulta consolidada dos totais financeiros.
/// </summary>
[ApiController]
[Route("api/totals")]
[Tags("Totais")]
public sealed class TotalsController : ControllerBase
{
    private readonly TotalsService _totalsService;

    public TotalsController(TotalsService totalsService)
    {
        _totalsService = totalsService;
    }

    /// <summary>
    /// Consulta os totais financeiros individuais e gerais.
    /// </summary>
    /// <remarks>
    /// Para cada pessoa cadastrada, a consulta apresenta:
    ///
    /// - total de receitas;
    /// - total de despesas;
    /// - saldo individual.
    ///
    /// Pessoas sem transações também são apresentadas, com valores zerados.
    ///
    /// Ao final da resposta, o campo general apresenta o total de receitas,
    /// despesas e o saldo líquido de todas as pessoas.
    /// </remarks>
    /// <param name="cancellationToken">
    /// Token utilizado para cancelar a operação.
    /// </param>
    /// <returns>
    /// Totais financeiros por pessoa e o consolidado geral.
    /// </returns>
    /// <response code="200">
    /// Totais calculados com sucesso.
    /// </response>
    [HttpGet]
    [EndpointName("GetTotals")]
    [ProducesResponseType(
        typeof(TotalsResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<TotalsResponse>> Get(
        CancellationToken cancellationToken)
    {
        var totals = await _totalsService.GetAsync(
            cancellationToken);

        return Ok(totals);
    }
}