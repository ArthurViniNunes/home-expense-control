using HomeExpenseControl.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using HomeExpenseControl.Api.Features.People;
using HomeExpenseControl.Api.Common.Errors;
using HomeExpenseControl.Api.Features.Transactions;
using HomeExpenseControl.Api.Common.OpenApi;
using HomeExpenseControl.Api.Features.Totals;
using HomeExpenseControl.Api.Common.Cors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Impede que os controllers aceitem números como strings.
        // Exemplo inválido: { "personId": "3" }
        options.JsonSerializerOptions.NumberHandling =
            JsonNumberHandling.Strict;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory =
            ApiValidationProblemDetailsFactory.Create;
    });

builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Faz o gerador OpenAPI representar números somente como números,
    // removendo os tipos alternativos string e os patterns numéricos.
    options.SerializerOptions.NumberHandling =
        JsonNumberHandling.Strict;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer<OpenApiSchemaConventionTransformer>();

    options.AddDocumentTransformer(
        (document, context, cancellationToken) =>
        {
            document.Info.Title =
                "Controle de Gastos Residenciais API";

            document.Info.Version = "v1";

            document.Info.Description =
                "API para cadastro de pessoas, transações " +
                "e consulta de totais financeiros residenciais.";

            return Task.CompletedTask;
        });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var connectionString = builder.Configuration.GetConnectionString(
    "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "A connection string 'DefaultConnection' não foi configurada.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<PeopleService>();
builder.Services.AddScoped<TransactionsService>();
builder.Services.AddScoped<TotalsService>();


// Cors
var frontendOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

if (builder.Environment.IsDevelopment()
    && (frontendOrigins is null || frontendOrigins.Length == 0))
{
    throw new InvalidOperationException(
        "Nenhuma origem do front-end foi configurada em " +
        "'Cors:AllowedOrigins'.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        CorsPolicyNames.Frontend,
        policy =>
        {
            policy
                .WithOrigins(frontendOrigins ?? [])
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .AddDocument(
                "v1",
                "Controle de Gastos Residenciais API")
            .WithTitle(
                "Controle de Gastos Residenciais — Referência da API")
            .WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient)
            .DisableAgent();
    });
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors(CorsPolicyNames.Frontend);

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;