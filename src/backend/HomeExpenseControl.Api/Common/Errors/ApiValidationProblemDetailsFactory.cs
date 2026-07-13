using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HomeExpenseControl.Api.Common.Errors;

/// <summary>
/// Cria respostas padronizadas para erros automáticos de validação
/// e de conversão do corpo da requisição.
/// </summary>
public static class ApiValidationProblemDetailsFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var errors = BuildErrors(context.ModelState);

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Title = "Dados inválidos",
            Detail = "Um ou mais campos possuem valores inválidos.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id
            ?? context.HttpContext.TraceIdentifier;

        var result = new BadRequestObjectResult(problemDetails);

        result.ContentTypes.Add("application/problem+json");

        return result;
    }

    private static Dictionary<string, string[]> BuildErrors(
        ModelStateDictionary modelState)
    {
        var hasSpecificFieldError = modelState.Any(entry =>
            entry.Value?.Errors.Count > 0
            && !IsRequestBodyKey(entry.Key));

        var normalizedErrors = new Dictionary<
            string,
            List<string>>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var entry in modelState)
        {
            if (entry.Value is null || entry.Value.Errors.Count == 0)
            {
                continue;
            }

            // Quando a desserialização de uma propriedade falha, o ASP.NET
            // também pode adicionar um erro genérico para o parâmetro request.
            // O erro genérico é removido quando já existe um erro mais preciso.
            if (hasSpecificFieldError && IsRequestBodyKey(entry.Key))
            {
                continue;
            }

            var field = NormalizeFieldName(entry.Key);

            if (!normalizedErrors.TryGetValue(
                field,
                out var fieldErrors))
            {
                fieldErrors = [];
                normalizedErrors[field] = fieldErrors;
            }

            foreach (var error in entry.Value.Errors)
            {
                var message = ResolveMessage(field, error);

                if (!fieldErrors.Contains(
                    message,
                    StringComparer.OrdinalIgnoreCase))
                {
                    fieldErrors.Add(message);
                }
            }
        }

        return normalizedErrors.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.ToArray(),
            StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeFieldName(string key)
    {
        if (IsRequestBodyKey(key))
        {
            return "body";
        }

        var normalizedKey = key.StartsWith(
            "$.",
            StringComparison.Ordinal)
            ? key[2..]
            : key;

        if (normalizedKey.Length == 0)
        {
            return "body";
        }

        return char.ToLowerInvariant(normalizedKey[0])
            + normalizedKey[1..];
    }

    private static bool IsRequestBodyKey(string key)
    {
        return string.IsNullOrWhiteSpace(key)
            || key.Equals(
                "request",
                StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveMessage(
        string field,
        ModelError error)
    {
        var originalMessage = error.ErrorMessage;

        if (field == "body"
            && IsRequiredRequestBodyMessage(originalMessage))
        {
            return "O corpo da requisição é obrigatório.";
        }

        if (ContainsJsonException(error.Exception)
            || IsJsonConversionMessage(originalMessage))
        {
            return GetJsonConversionMessage(field);
        }

        if (IsFrameworkRequiredMessage(originalMessage))
        {
            return field == "body"
                ? "O corpo da requisição é obrigatório."
                : $"O campo '{field}' é obrigatório.";
        }

        if (!string.IsNullOrWhiteSpace(originalMessage))
        {
            return originalMessage;
        }

        return $"O valor informado para o campo '{field}' é inválido.";
    }

    private static string GetJsonConversionMessage(string field)
    {
        return field switch
        {
            "type" =>
                "O tipo deve ser 'expense' para despesa " +
                "ou 'income' para receita.",

            "personId" =>
                "O identificador da pessoa deve ser um número inteiro.",

            "age" =>
                "A idade deve ser um número inteiro.",

            "amount" =>
                "O valor deve ser informado como um número válido.",

            _ =>
                $"O valor informado para o campo '{field}' " +
                "possui formato inválido."
        };
    }

    private static bool ContainsJsonException(Exception? exception)
    {
        while (exception is not null)
        {
            if (exception is JsonException)
            {
                return true;
            }

            exception = exception.InnerException;
        }

        return false;
    }

    private static bool IsJsonConversionMessage(string message)
    {
        return message.Contains(
            "could not be converted",
            StringComparison.OrdinalIgnoreCase)
            || message.Contains(
                "invalid JSON",
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRequiredRequestBodyMessage(string message)
    {
        return message.Contains(
            "request field is required",
            StringComparison.OrdinalIgnoreCase)
            || message.Contains(
                "non-empty request body is required",
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFrameworkRequiredMessage(string message)
    {
        return message.Contains(
            "field is required",
            StringComparison.OrdinalIgnoreCase)
            || message.Contains(
                "field is required.",
                StringComparison.OrdinalIgnoreCase);
    }
}