using System.Net;
using Xunit;

namespace VirGo.SecurityTests;

public sealed class SecurityHeaderTests(SecurityApiFactory factory) : IClassFixture<SecurityApiFactory>
{
    [Fact]
    public async Task HealthHasSecurityHeaders()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
    }

    [Fact]
    public async Task DisallowedOriginDoesNotReceiveCorsPermission()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "https://not-allowed.example");

        using var response = await client.SendAsync(request);

        Assert.DoesNotContain("Access-Control-Allow-Origin", response.Headers.Select(header => header.Key));
    }
}
