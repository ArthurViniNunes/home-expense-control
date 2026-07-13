using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HomeExpenseControl.Api.Common.Errors;

/// <summary>
/// Converte exceções conhecidas da aplicação em respostas HTTP padronizadas.
/// </summary>

// A ideia aqui é esconder o StackTrace do usuário final, mas ainda assim logar o erro para que possamos investigar o problema.

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            ResourceNotFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                exception.Message),

            BusinessRuleException => (
                StatusCodes.Status422UnprocessableEntity,
                "Regra de negócio violada",
                exception.Message),

            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Dados inválidos",
                exception.Message),

            OverflowException => (
                StatusCodes.Status400BadRequest,
                "Valor fora do limite permitido",
                "O valor informado excede o limite suportado pela aplicação."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno",
                "Ocorreu um erro inesperado durante o processamento da solicitação.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Erro inesperado ao processar {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}