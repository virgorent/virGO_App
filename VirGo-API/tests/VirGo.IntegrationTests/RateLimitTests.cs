using System.Net;
using Xunit;

namespace VirGo.IntegrationTests;

public sealed class RateLimitTests(RateLimitFactory factory) : IClassFixture<RateLimitFactory>
{
    [Fact]
    public async Task SecondRequestInWindowReturnsProblemDetails429()
    {
        using var client = factory.CreateClient();

        using var first = await client.GetAsync("/health");
        using var second = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, second.StatusCode);
        Assert.Equal("application/problem+json", second.Content.Headers.ContentType?.MediaType);
    }
}
