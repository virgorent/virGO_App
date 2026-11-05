using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using VirGo.Application.Abstractions;
using VirGo.Application.Auth;

namespace VirGo.Api.Security;

public sealed class JwtSessionEvents : JwtBearerEvents
{
    private readonly ISessionRepository sessions;
    private readonly IClock clock;

    public JwtSessionEvents(ISessionRepository sessions, IClock clock)
    {
        this.sessions = sessions;
        this.clock = clock;
        OnTokenValidated = ValidateSession;
    }

    public async Task ValidateSession(TokenValidatedContext context)
    {
        var principal = context.Principal;
        var userId = principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var sessionId = principal?.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(sessionId))
        {
            context.Fail("The access token has no session identity.");
            return;
        }

        var session = await sessions.FindAsync(sessionId, userId, context.HttpContext.RequestAborted);
        if (session is null || session.IsExpired(clock.UtcNow))
        {
            context.Fail("The session is invalid or expired.");
        }
    }
}
