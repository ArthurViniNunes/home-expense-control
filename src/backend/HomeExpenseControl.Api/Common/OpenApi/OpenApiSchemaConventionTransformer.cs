using System.Text.Json.Nodes;
using HomeExpenseControl.Api.Domain.Enums;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HomeExpenseControl.Api.Common.OpenApi;

/// <summary>
/// Padroniza a representação dos tipos da aplicação no documento OpenAPI.
/// </summary>
public sealed class OpenApiSchemaConventionTransformer
    : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var declaredType =
            context.JsonPropertyInfo?.PropertyType
            ?? context.JsonTypeInfo.Type;

        var actualType =
            Nullable.GetUnderlyingType(declaredType)
            ?? declaredType;

        if (actualType == typeof(TransactionType))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = null;
            schema.Pattern = null;

            schema.Enum =
            [
                JsonValue.Create("expense")!,
                JsonValue.Create("income")!
            ];

            schema.Description =
                "Tipo da transação. Use 'expense' para despesa " +
                "ou 'income' para receita.";
        }

        if (actualType == typeof(decimal))
        {
            schema.Type = JsonSchemaType.Number;
            schema.Format = "decimal";
            schema.Pattern = null;
        }

        return Task.CompletedTask;
    }
}