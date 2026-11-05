using VirGo.Application.Abstractions;
using VirGo.Domain.Auth;

namespace VirGo.Application.Auth;

public sealed class LoginHandler
{
    private const int LockoutThreshold = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private const int RefreshTokenDays = 30;
    private readonly IUserRepository users;
    private readonly ISessionRepository sessions;
    private readonly IPasswordHasher passwordHasher;
    private readonly ITokenService tokenService;
    private readonly IRefreshTokenGenerator refreshTokenGenerator;
    private readonly IClock clock;
    private readonly User fakeUser;

    public LoginHandler(
        IUserRepository users,
        ISessionRepository sessions,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenGenerator refreshTokenGenerator,
        IClock clock)
    {
        this.users = users;
        this.sessions = sessions;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
        this.refreshTokenGenerator = refreshTokenGenerator;
        this.clock = clock;
        fakeUser = new User
        {
            Id = "00000000-0000-0000-0000-000000000000",
            Name = "Invalid user",
            Email = "invalid@example.invalid",
            Phone = "0000000000",
            PasswordHash = string.Empty,
            Role = UserRole.Client,
            CreatedAt = DateTime.UnixEpoch,
            UpdatedAt = DateTime.UnixEpoch
        };
        fakeUser.PasswordHash = passwordHasher.Hash(fakeUser, "invalid-login-password");
    }

    public async Task<LoginOutcome> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return new(null, LoginFailure.InvalidRequest);
        }

        var user = await users.FindByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            passwordHasher.Verify(fakeUser, fakeUser.PasswordHash, command.Password);
            return new(null, LoginFailure.InvalidCredentials);
        }

        var now = clock.UtcNow;
        if (user.LockoutEndsAt is { } lockoutEndsAt && lockoutEndsAt > now)
        {
            return new(null, LoginFailure.LockedOut, RetryAfterSeconds(lockoutEndsAt, now));
        }

        if (!passwordHasher.Verify(user, user.PasswordHash, command.Password))
        {
            await users.RecordFailedLoginAsync(
                user.Id,
                now,
                LockoutThreshold,
                LockoutDuration,
                cancellationToken);
            return new(null, LoginFailure.InvalidCredentials);
        }

        if (user.FailedLoginCount > 0 || user.LockoutEndsAt is not null)
        {
            await users.ResetLoginFailuresAsync(user.Id, now, cancellationToken);
        }

        var refreshToken = refreshTokenGenerator.Create();
        var session = new Session
        {
            Id = Guid.NewGuid().ToString("D"),
            UserId = user.Id,
            TokenHash = refreshTokenGenerator.Hash(refreshToken),
            CreatedAt = now,
            ExpiresAt = now.AddDays(RefreshTokenDays),
            PurgeAt = now.AddDays(RefreshTokenDays + 7)
        };
        await sessions.AddAsync(session, cancellationToken);

        var accessToken = tokenService.CreateAccessToken(user, session, now);
        return new(new LoginResult(accessToken, refreshToken, session.ExpiresAt, user), null);
    }

    private static int RetryAfterSeconds(DateTime lockoutEndsAt, DateTime now) =>
        Math.Max(1, (int)Math.Ceiling((lockoutEndsAt - now).TotalSeconds));
}
