using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace VirGo.IntegrationTests;

public sealed class BaseApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task HealthReturnsOk()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task OpenApiIsPublished()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Bearer", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MissingRouteReturnsProblemDetailsWithTraceId()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/not-a-route");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("traceId", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProtectedRouteWithoutTokenReturnsUnauthorizedProblemDetails()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/_diagnostics/secure");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task UnexpectedFailureReturnsSafeProblemDetails()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/_diagnostics/failure");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain("InvalidOperationException", body, StringComparison.Ordinal);
        Assert.Contains("traceId", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResponsesIncludeCorrelationAndSecurityHeaders()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-Id", "integration-test");

        using var response = await client.SendAsync(request);

        Assert.Equal("integration-test", response.Headers.GetValues("X-Correlation-Id").Single());
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
    }
}
