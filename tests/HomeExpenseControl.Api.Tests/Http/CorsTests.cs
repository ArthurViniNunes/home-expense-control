using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace HomeExpenseControl.Api.Tests.Http;

public sealed class CorsTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string AllowedOrigin =
        "http://localhost:5173";

    private readonly HttpClient _client;

    public CorsTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");

                builder.ConfigureAppConfiguration(
                    (_, configuration) =>
                    {
                        configuration.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["Cors:AllowedOrigins:0"] =
                                    AllowedOrigin
                            });
                    });
            })
            .CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress =
                        new Uri("https://localhost")
                });
    }

    [Fact]
    public async Task Preflight_ShouldAllowConfiguredFrontendOrigin()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/api/people");

        request.Headers.Add(
            "Origin",
            AllowedOrigin);

        request.Headers.Add(
            "Access-Control-Request-Method",
            "POST");

        request.Headers.Add(
            "Access-Control-Request-Headers",
            "content-type");

        using var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        Assert.Equal(
            AllowedOrigin,
            response.Headers
                .GetValues("Access-Control-Allow-Origin")
                .Single());

        Assert.Contains(
            "POST",
            response.Headers
                .GetValues("Access-Control-Allow-Methods")
                .Single(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Preflight_ShouldNotAllowUnconfiguredOrigin()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/api/people");

        request.Headers.Add(
            "Origin",
            "https://origem-nao-permitida.example");

        request.Headers.Add(
            "Access-Control-Request-Method",
            "POST");

        request.Headers.Add(
            "Access-Control-Request-Headers",
            "content-type");

        using var response =
            await _client.SendAsync(request);

        Assert.False(
            response.Headers.Contains(
                "Access-Control-Allow-Origin"));

        Assert.False(
            response.Headers.Contains(
                "Access-Control-Allow-Methods"));

        Assert.False(
            response.Headers.Contains(
                "Access-Control-Allow-Headers"));
    }
}