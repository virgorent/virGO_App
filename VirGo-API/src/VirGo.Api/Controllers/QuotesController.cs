using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirGo.Application.Auth;
using VirGo.Application.Marketplace;

namespace VirGo.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/quotes")]
public sealed class QuotesController(QuoteHandler handler, IUserRepository users) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateQuoteRequest request, CancellationToken cancellationToken)
    {
        var user = await GetCurrentUser(cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Target?.Id)
            || !Guid.TryParse(request.Target.Id, out _)
            || !string.Equals(request.Target.Type, "venue", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(request.EventType)
            || !AllowedEventTypes.Contains(request.EventType, StringComparer.Ordinal)
            || request.Guests < 1
            || !DateOnly.TryParseExact(request.EventDate, "yyyy-MM-dd", out var eventDate)
            || request.EventId is not null && !Guid.TryParse(request.EventId, out _)
            || request.RequestedServices is not null && request.RequestedServices.Any(service => string.IsNullOrWhiteSpace(service) || service.Length > 100)
            || request.Message is not null && request.Message.Length > 2000)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "La solicitud de cotización es inválida.");
        }

        var result = await handler.HandleAsync(new CreateQuoteCommand(
            user.Id,
            user.Name,
            request.Target.Type ?? "venue",
            request.Target.Id,
            request.EventId ?? string.Empty,
            request.EventType,
            eventDate,
            request.Guests,
            request.RequestedServices ?? [],
            request.Message), cancellationToken);
        if (result.InvalidInput)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "La solicitud de cotización es inválida.");
        }

        return result.Quote is null
            ? NotFound()
            : Created($"/api/v1/quotes/{result.Quote.Id}", result.Quote);
    }

    private Task<VirGo.Domain.Auth.User?> GetCurrentUser(CancellationToken cancellationToken) =>
        users.FindByIdAsync(User.FindFirstValue("sub")!, cancellationToken);

    private static readonly string[] AllowedEventTypes = ["Boda", "XV años", "Cumpleaños", "Corporativo", "Otro"];
}

public sealed record CreateQuoteRequest(
    QuoteTargetRequest? Target,
    string? EventId,
    string? EventType,
    string? EventDate,
    int Guests,
    string[]? RequestedServices,
    string? Message);

public sealed record QuoteTargetRequest(string? Type, string? Id);
