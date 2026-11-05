using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace VirGo.SecurityTests;

public sealed class JwtSecurityTests(SecurityApiFactory factory) : IClassFixture<SecurityApiFactory>
{
    [Fact]
    public async Task TokenWithoutSessionIdIsRejected()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("DevelopmentOnlySigningKeyNotASecret123456789"));
        var token = new JwtSecurityToken(
            issuer: "VirGo.Api",
            audience: "VirGo.Mobile",
            claims: [new Claim(JwtRegisteredClaimNames.Sub, "8f6b8b48-f5d0-4f65-8fb0-2a7f0f6ab4b1")],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var rawToken = new JwtSecurityTokenHandler().WriteToken(token);

        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/_diagnostics/secure");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", rawToken);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
