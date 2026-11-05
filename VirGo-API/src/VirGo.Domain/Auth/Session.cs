namespace VirGo.Domain.Auth;

public sealed class Session
{
    public required string Id { get; init; }

    public required string UserId { get; init; }

    public required string TokenHash { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime ExpiresAt { get; init; }

    public DateTime PurgeAt { get; init; }

    public bool IsExpired(DateTime utcNow) => ExpiresAt <= utcNow;
}
