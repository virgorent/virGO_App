using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace VirGo.IntegrationTests;

public sealed class VerticalSliceTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string[] RequestedServices = ["Banquete"];

    [Fact]
    public async Task RegisterCreatesAClientAccount()
    {
        using var client = factory.CreateClient();
        var email = $"new-{Guid.NewGuid():N}@virgo.mx";

        using var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Nueva Persona",
            email,
            password = "Strong-password-123",
            phone = "+525500000000"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(email, json.RootElement.GetProperty("email").GetString());
        Assert.Equal("client", json.RootElement.GetProperty("role").GetString());
    }

    [Fact]
    public async Task RegisterRejectsDuplicateEmail()
    {
        using var client = factory.CreateClient();
        var email = $"duplicate-{Guid.NewGuid():N}@virgo.mx";
        var request = new
        {
            name = "Nueva Persona",
            email,
            password = "Strong-password-123",
            phone = "+525500000000"
        };

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/v1/auth/register", request)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/v1/auth/register", request)).StatusCode);
    }

    [Fact]
    public async Task RegisterRejectsInvalidInputAsProblemDetails()
    {
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "",
            email = "not-an-email",
            password = "short",
            phone = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task AuthenticatedUserCanReadOwnProfile()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/api/v1/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("demo@virgo.mx", json.RootElement.GetProperty("email").GetString());
    }

    [Fact]
    public async Task ProfileRejectsAnInvalidToken()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        using var response = await client.GetAsync("/api/v1/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PublicVenueCatalogReturnsPublishedVenues()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/venues?zone=Centro");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.NotEmpty(json.RootElement.GetProperty("data").EnumerateArray());
    }

    [Fact]
    public async Task PublicVenueCatalogFiltersBySchemaCategory()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/venues?category=Salon");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.NotEmpty(json.RootElement.GetProperty("data").EnumerateArray());
        Assert.DoesNotContain("ownerId", json.RootElement.GetProperty("data")[0].EnumerateObject().Select(property => property.Name));
    }

    [Fact]
    public async Task PublicVenueCatalogReturnsNoResultsForUnknownCategory()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/venues?category=Rooftop");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(json.RootElement.GetProperty("data").EnumerateArray());
    }

    [Fact]
    public async Task QuoteRejectsNullRequestedService()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.PostAsJsonAsync("/api/v1/quotes", new
        {
            target = new { type = "venue", id = "11111111-1111-4111-8111-111111111111" },
            eventType = "Boda",
            eventDate = "2027-05-15",
            guests = 150,
            requestedServices = new string?[] { null }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task QuoteRequiresAuthentication()
    {
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/v1/quotes", new
        {
            target = new { type = "venue", id = "11111111-1111-4111-8111-111111111111" },
            eventType = "Boda",
            eventDate = "2027-05-15",
            guests = 150,
            requestedServices = RequestedServices
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task QuoteRejectsPastEventDate()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.PostAsJsonAsync("/api/v1/quotes", new
        {
            target = new { type = "venue", id = "11111111-1111-4111-8111-111111111111" },
            eventType = "Boda",
            eventDate = "2020-01-01",
            guests = 150,
            requestedServices = RequestedServices
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task QuoteReturnsNotFoundForUnknownVenue()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.PostAsJsonAsync("/api/v1/quotes", new
        {
            target = new { type = "venue", id = "99999999-9999-4999-8999-999999999999" },
            eventType = "Boda",
            eventDate = "2027-05-15",
            guests = 150,
            requestedServices = RequestedServices
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedClientCanCreateQuoteAndBooking()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var quoteResponse = await client.PostAsJsonAsync("/api/v1/quotes", new
        {
            target = new { type = "venue", id = "11111111-1111-4111-8111-111111111111" },
            eventId = "22222222-2222-4222-8222-222222222222",
            eventType = "Boda",
            eventDate = "2027-05-15",
            guests = 150,
            requestedServices = RequestedServices,
            message = "Solicito una cotización."
        });
        Assert.Equal(HttpStatusCode.Created, quoteResponse.StatusCode);

        using var bookingRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bookings")
        {
            Content = JsonContent.Create(new
            {
                quoteId = Guid.NewGuid().ToString("D"),
                eventId = "22222222-2222-4222-8222-222222222222",
                venueId = "11111111-1111-4111-8111-111111111111",
                date = "2027-05-15",
                hours = 1,
                deposit = 1000
            })
        };
        bookingRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("D"));
        using var bookingResponse = await client.SendAsync(bookingRequest);
        Assert.Equal(HttpStatusCode.Created, bookingResponse.StatusCode);
        using var bookingJson = JsonDocument.Parse(await bookingResponse.Content.ReadAsStringAsync());
        var bookingId = bookingJson.RootElement.GetProperty("id").GetString();
        Assert.Equal("pending_payment", bookingJson.RootElement.GetProperty("status").GetString());

        using var replayRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bookings")
        {
            Content = JsonContent.Create(new
            {
                quoteId = Guid.NewGuid().ToString("D"),
                eventId = "22222222-2222-4222-8222-222222222222",
                venueId = "11111111-1111-4111-8111-111111111111",
                date = "2027-05-15",
                hours = 1,
                deposit = 1000
            })
        };
        replayRequest.Headers.Add("Idempotency-Key", bookingRequest.Headers.GetValues("Idempotency-Key").Single());
        using var replayResponse = await client.SendAsync(replayRequest);
        Assert.Equal(HttpStatusCode.Created, replayResponse.StatusCode);
        using var replayJson = JsonDocument.Parse(await replayResponse.Content.ReadAsStringAsync());
        Assert.Equal(bookingId, replayJson.RootElement.GetProperty("id").GetString());
    }

    [Fact]
    public async Task BookingRejectsDepositAboveCalculatedTotal()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bookings")
        {
            Content = JsonContent.Create(new
            {
                venueId = "11111111-1111-4111-8111-111111111111",
                date = "2027-05-15",
                hours = 1,
                deposit = 45001
            })
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("D"));

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BookingCalculatesDayPriceAndDefaultDeposit()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bookings")
        {
            Content = JsonContent.Create(new
            {
                venueId = "11111111-1111-4111-8111-111111111111",
                date = "2027-05-15",
                hours = 25
            })
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("D"));

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(90000, json.RootElement.GetProperty("total").GetDecimal());
        Assert.Equal(27000, json.RootElement.GetProperty("deposit").GetDecimal());
    }

    [Fact]
    public async Task ConcurrentBookingRequestsWithTheSameKeyReturnOneBooking()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var idempotencyKey = Guid.NewGuid().ToString("D");

        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bookings")
            {
                Content = JsonContent.Create(new
                {
                    venueId = "11111111-1111-4111-8111-111111111111",
                    date = "2027-05-15",
                    hours = 1,
                    deposit = 1000
                })
            };
            request.Headers.Add("Idempotency-Key", idempotencyKey);
            return await client.SendAsync(request);
        }));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));
        var payloads = await Task.WhenAll(responses.Select(async response =>
        {
            using (response)
            {
                using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return (
                    Id: payload.RootElement.GetProperty("id").GetString(),
                    Folio: payload.RootElement.GetProperty("folio").GetString());
            }
        }));
        var bookingIds = payloads.Select(payload => payload.Id).Distinct().ToArray();
        var folios = payloads.Select(payload => payload.Folio).Distinct().ToArray();
        Assert.Single(bookingIds);
        Assert.Single(folios);
    }

    [Fact]
    public async Task LoginLocksAnAccountAfterFiveFailedAttempts()
    {
        using var client = factory.CreateClient();
        var email = $"lockout-{Guid.NewGuid():N}@virgo.mx";
        var password = "Strong-password-123";

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Cuenta bloqueable",
            email,
            password,
            phone = "+525500000000"
        })).StatusCode);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var failed = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "wrong-password" });
            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        using var locked = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.TooManyRequests, locked.StatusCode);
        Assert.True(locked.Headers.Contains("Retry-After"));
    }

    [Fact]
    public async Task BookingRequiresIdempotencyKey()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.PostAsJsonAsync("/api/v1/bookings", new
        {
            venueId = "11111111-1111-4111-8111-111111111111",
            date = "2027-05-15",
            hours = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
