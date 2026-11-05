namespace VirGo.Application.Auth;

public sealed class LogoutHandler(ISessionRepository sessions)
{
    public async Task<bool> HandleAsync(string userId, string sessionId, CancellationToken cancellationToken = default)
    {
        return await sessions.DeleteAsync(sessionId, userId, cancellationToken);
    }
}
