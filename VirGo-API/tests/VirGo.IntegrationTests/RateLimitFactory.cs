using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VirGo.IntegrationTests;

public sealed class RateLimitFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("RateLimiting:PermitLimit", "1");
        builder.UseSetting("RateLimiting:WindowSeconds", "60");
        builder.UseSetting("Jwt:SigningKey", "TestingOnlySigningKeyMustBeAtLeast32Characters");
    }
}
