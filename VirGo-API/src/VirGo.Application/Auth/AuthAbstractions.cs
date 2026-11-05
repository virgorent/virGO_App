using VirGo.Domain.Auth;

namespace VirGo.Application.Auth;

public sealed class DuplicateUserEmailException : Exception
{
    public DuplicateUserEmailException() : base("The user email already exists.")
    {
    }
}

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(string id, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> RecordFailedLoginAsync(
        string userId,
        DateTime now,
        int lockoutThreshold,
        TimeSpan lockoutDuration,
        CancellationToken cancellationToken = default);

    Task ResetLoginFailuresAsync(string userId, DateTime now, CancellationToken cancellationToken = default);
}

public interface ISessionRepository
{
    Task<Session?> FindAsync(string sessionId, string userId, CancellationToken cancellationToken = default);

    Task AddAsync(Session session, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string sessionId, string userId, CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string Hash(User user, string password);

    bool Verify(User user, string passwordHash, string password);
}

public interface ITokenService
{
    string CreateAccessToken(User user, Session session, DateTime utcNow);
}

public interface IRefreshTokenGenerator
{
    string Create();

    string Hash(string token);
}
