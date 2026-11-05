using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirGo.Application.Auth;
using VirGo.Application.Marketplace;

namespace VirGo.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/bookings")]
public sealed class BookingsController(BookingHandler handler, IUserRepository users) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        var user = string.IsNullOrWhiteSpace(userId) ? null : await users.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey)
            || string.IsNullOrWhiteSpace(idempotencyKey.ToString())
            || idempotencyKey.ToString().Length > 128)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Idempotency-Key es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.VenueId)
            || !Guid.TryParse(request.VenueId, out _)
            || string.IsNullOrWhiteSpace(request.Date)
            || !DateOnly.TryParseExact(request.Date, "yyyy-MM-dd", out _)
            || request.QuoteId is not null && !Guid.TryParse(request.QuoteId, out _)
            || request.EventId is not null && !Guid.TryParse(request.EventId, out _)
            || request.Hours < 1)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "La solicitud de reservación es inválida.");
        }

        var result = await handler.HandleAsync(new CreateBookingCommand(
            user.Id,
            idempotencyKey.ToString().Trim(),
            user.Name,
            request.QuoteId,
            request.EventId,
            request.VenueId,
            request.Date,
            request.Hours,
            request.Deposit), cancellationToken);
        if (result.InvalidInput)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "La solicitud de reservación es inválida.");
        }

        return result.Booking is null
            ? NotFound()
            : Created($"/api/v1/bookings/{result.Booking.Id}", result.Booking);
    }
}

public sealed record CreateBookingRequest(
    string? QuoteId,
    string? EventId,
    string? VenueId,
    string? Date,
    int Hours,
    decimal? Deposit);
