using System.Globalization;
using VirGo.Application.Abstractions;
using VirGo.Domain.Marketplace;

namespace VirGo.Application.Marketplace;

public sealed class BookingHandler(IVenueRepository venues, IBookingRepository bookings, IClock clock)
{
    public async Task<BookingCreationResult> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await bookings.FindByIdempotencyKeyAsync(command.ClientId, command.IdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return new(existing, false);
        }

        if (!Guid.TryParse(command.VenueId, out _)
            || command.IdempotencyKey.Length > 128
            || command.Hours < 1
            || !DateOnly.TryParseExact(command.Date, "yyyy-MM-dd", out var date))
        {
            return new(null, true);
        }

        var venue = await venues.FindPublishedAsync(command.VenueId, cancellationToken);
        if (venue is null)
        {
            return new(null, false);
        }

        if (date < DateOnly.FromDateTime(clock.UtcNow))
        {
            return new(null, true);
        }

        var now = clock.UtcNow;
        if (command.Hours < venue.MinimumHours)
        {
            return new(null, true);
        }

        var billingUnits = string.Equals(venue.PriceUnit, "day", StringComparison.OrdinalIgnoreCase)
            ? (int)Math.Ceiling(command.Hours / 24m)
            : command.Hours;
        var total = venue.BasePrice * billingUnits;
        var deposit = command.Deposit ?? Math.Round(total * 0.30m, 2, MidpointRounding.AwayFromZero);
        if (deposit < 0 || deposit > total)
        {
            return new(null, true);
        }

        var folio = await bookings.NextFolioAsync(now.Year, cancellationToken);
        var booking = new Booking
        {
            Id = Guid.NewGuid().ToString("D"),
            ClientId = command.ClientId,
            IdempotencyKey = command.IdempotencyKey,
            ClientName = command.ClientName,
            QuoteId = string.IsNullOrWhiteSpace(command.QuoteId) ? null : command.QuoteId,
            EventId = string.IsNullOrWhiteSpace(command.EventId) ? null : command.EventId,
            VenueId = venue.Id,
            VenueName = venue.Name,
            Date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Hours = command.Hours,
            Total = total,
            Deposit = deposit,
            Folio = folio,
            Status = "pending_payment",
            CreatedAt = now,
            UpdatedAt = now
        };
        try
        {
            await bookings.AddAsync(booking, cancellationToken);
        }
        catch (DuplicateBookingIdempotencyException)
        {
            return new(await bookings.FindByIdempotencyKeyAsync(command.ClientId, command.IdempotencyKey, cancellationToken), false);
        }

        return new(booking, false);
    }
}
