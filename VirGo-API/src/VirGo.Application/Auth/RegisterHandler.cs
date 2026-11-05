using VirGo.Application.Abstractions;
using VirGo.Domain.Auth;

namespace VirGo.Application.Auth;

public sealed class RegisterHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IClock clock)
{
    public async Task<RegisterOutcome> HandleAsync(
        RegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(command.Name)
            || command.Name.Trim().Length > 100
            || string.IsNullOrWhiteSpace(email)
            || email.Length > 150
            || string.IsNullOrWhiteSpace(command.Password)
            || command.Password.Length > 128
            || string.IsNullOrWhiteSpace(command.Phone)
            || command.Phone.Trim().Length > 16)
        {
            return new(null, RegisterFailure.InvalidRequest);
        }

        if (await users.FindByEmailAsync(email, cancellationToken) is not null)
        {
            return new(null, RegisterFailure.EmailAlreadyExists);
        }

        var now = clock.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid().ToString("D"),
            Name = command.Name.Trim(),
            Email = email,
            Phone = command.Phone.Trim(),
            PasswordHash = string.Empty,
            Role = UserRole.Client,
            IsVerified = false,
            CreatedAt = now,
            UpdatedAt = now
        };
        user.PasswordHash = passwordHasher.Hash(user, command.Password);
        await users.AddAsync(user, cancellationToken);
        return new(user, null);
    }
}
