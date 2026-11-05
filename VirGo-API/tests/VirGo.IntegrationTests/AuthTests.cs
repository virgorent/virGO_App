using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace VirGo.IntegrationTests;

public sealed class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task LoginThenLogoutInvalidatesOnlyThatSession()
    {
        using var client = factory.CreateClient();
        using var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "demo@virgo.mx", password = "VirGo-local-only-password" });
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        using var loginJson = JsonDocument.Parse(loginBody);
        var accessToken = loginJson.RootElement.GetProperty("accessToken").GetString();

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        using var authorizedRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/_diagnostics/secure");
        authorizedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var authorizedResponse = await client.SendAsync(authorizedRequest);
        Assert.Equal(HttpStatusCode.OK, authorizedResponse.StatusCode);

        using var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        logoutRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var logoutResponse = await client.SendAsync(logoutRequest);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        using var rejectedRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/_diagnostics/secure");
        rejectedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var rejectedResponse = await client.SendAsync(rejectedRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, rejectedResponse.StatusCode);
    }

    [Fact]
    public async Task UnknownEmailReturnsGenericUnauthorizedResponse()
    {
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "unknown@virgo.mx", password = "wrong-password" });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Correo o contraseña incorrectos", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoginNormalizesEmailCaseAndWhitespace()
    {
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "  DEMO@VIRGO.MX  ", password = "VirGo-local-only-password" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task LoginRejectsInvalidInputWithFieldErrors()
    {
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "not-an-email", password = "" });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("email", body, StringComparison.Ordinal);
        Assert.Contains("password", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LogoutKeepsASecondSessionActive()
    {
        using var client = factory.CreateClient();
        var firstToken = await LoginAsync(client);
        var secondToken = await LoginAsync(client);

        using var logout = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        logout.Headers.Authorization = new AuthenticationHeaderValue("Bearer", firstToken);
        Assert.Equal(HttpStatusCode.NoContent, (await client.SendAsync(logout)).StatusCode);

        using var secondRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/_diagnostics/secure");
        secondRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secondToken);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(secondRequest)).StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "demo@virgo.mx", password = "VirGo-local-only-password" });
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("accessToken").GetString()!;
    }
}
