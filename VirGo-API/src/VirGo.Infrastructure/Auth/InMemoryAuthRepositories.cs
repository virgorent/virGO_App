using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using VirGo.Application.Auth;
using VirGo.Application.Abstractions;
using VirGo.Domain.Auth;

namespace VirGo.Infrastructure.Auth;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> users = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryUserRepository(
        IConfiguration configuration,
        IPasswordHasher passwordHasher,
        IClock clock)
    {
        var email = (configuration["Authentication:DemoEmail"] ?? "demo@virgo.mx").Trim().ToLowerInvariant();
        var now = clock.UtcNow;
        var user = new User
        {
            Id = "8f6b8b48-f5d0-4f65-8fb0-2a7f0f6ab4b1",
            Name = "VirGo Demo",
            Email = email,
            Phone = "+524420000000",
            PasswordHash = string.Empty,
            Role = UserRole.Client,
            IsVerified = false,
            FailedLoginCount = 0,
            UnreadNotices = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
        user.PasswordHash = passwordHasher.Hash(
            user,
            configuration["Authentication:DemoPassword"] ?? "VirGo-local-only-password");
        users[email] = user;
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        users.TryGetValue(email, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(users.Values.FirstOrDefault(user => user.Id == id));
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!users.TryAdd(user.Email, user))
        {
            throw new DuplicateUserEmailException();
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        users[user.Email] = user;
        return Task.CompletedTask;
    }

    public Task<User?> RecordFailedLoginAsync(
        string userId,
        DateTime now,
        int lockoutThreshold,
        TimeSpan lockoutDuration,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = users.Values.FirstOrDefault(candidate => candidate.Id == userId);
        if (user is null)
        {
            return Task.FromResult<User?>(null);
        }

        lock (user)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= lockoutThreshold)
            {
                user.LockoutEndsAt = now.Add(lockoutDuration);
            }

            user.UpdatedAt = now;
        }

        return Task.FromResult<User?>(user);
    }

    public Task ResetLoginFailuresAsync(string userId, DateTime now, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = users.Values.FirstOrDefault(candidate => candidate.Id == userId);
        if (user is not null)
        {
            lock (user)
            {
                user.FailedLoginCount = 0;
                user.LockoutEndsAt = null;
                user.UpdatedAt = now;
            }
        }

        return Task.CompletedTask;
    }
}

public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<string, Session> sessions = new(StringComparer.Ordinal);

    public Task<Session?> FindAsync(string sessionId, string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            sessions.TryGetValue(sessionId, out var session) && session.UserId == userId
                ? session
                : null);
    }

    public Task AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!sessions.TryAdd(session.Id, session))
        {
            throw new InvalidOperationException("The session id already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string sessionId, string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!sessions.TryGetValue(sessionId, out var session) || session.UserId != userId)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(sessions.TryRemove(sessionId, out _));
    }
}
