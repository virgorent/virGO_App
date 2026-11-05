namespace VirGo.Domain.Auth;

public enum UserRole
{
    Client,
    Owner,
    Provider
}

public sealed class User
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public required string Phone { get; init; }

    public required string PasswordHash { get; set; }

    public required UserRole Role { get; init; }

    public bool IsVerified { get; init; }

    public int FailedLoginCount { get; set; }

    public DateTime? LockoutEndsAt { get; set; }

    public int UnreadNotices { get; set; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; set; }
}
