using System.ComponentModel.DataAnnotations;

namespace VirGo.Application.Configuration;

public sealed class PersistenceOptions
{
    public string Provider { get; init; } = "InMemory";

    public string DatabaseName { get; init; } = "virgo_dev";

    public string? ConnectionString { get; init; }
}

public sealed class JwtOptions
{
    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;
}

public sealed class CorsOptions
{
    public string[] AllowedOrigins { get; init; } = [];
}

public sealed class RateLimitingOptions
{
    [Range(1, 1000)]
    public int PermitLimit { get; init; } = 20;

    [Range(1, 3600)]
    public int WindowSeconds { get; init; } = 1;
}

public sealed class AuthenticationOptions
{
    public string DemoEmail { get; init; } = "demo@virgo.mx";

    public string DemoPassword { get; init; } = "VirGo-local-only-password";
}
