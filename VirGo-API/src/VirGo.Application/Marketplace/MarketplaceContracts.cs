using VirGo.Domain.Marketplace;

namespace VirGo.Application.Marketplace;

public sealed record CreateQuoteCommand(
    string ClientId,
    string ClientName,
    string TargetType,
    string TargetId,
    string EventId,
    string EventType,
    DateOnly EventDate,
    int Guests,
    string[] RequestedServices,
    string? Message);

public sealed record QuoteCreationResult(Quote? Quote, bool InvalidInput);

public sealed record CreateBookingCommand(
    string ClientId,
    string IdempotencyKey,
    string ClientName,
    string? QuoteId,
    string? EventId,
    string VenueId,
    string Date,
    int Hours,
    decimal? Deposit);

public sealed record BookingCreationResult(Booking? Booking, bool InvalidInput);
