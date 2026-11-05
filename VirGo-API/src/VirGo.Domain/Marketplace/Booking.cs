namespace VirGo.Domain.Marketplace;

public sealed class Booking
{
    public required string Id { get; init; }
    public required string ClientId { get; init; }
    public required string IdempotencyKey { get; init; }
    public required string ClientName { get; init; }
    public string? QuoteId { get; init; }
    public string? EventId { get; init; }
    public required string VenueId { get; init; }
    public required string VenueName { get; init; }
    public required string Date { get; init; }
    public required int Hours { get; init; }
    public required decimal Total { get; init; }
    public required decimal Deposit { get; init; }
    public required string Folio { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
