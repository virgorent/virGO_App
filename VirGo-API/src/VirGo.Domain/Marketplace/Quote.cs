namespace VirGo.Domain.Marketplace;

public sealed class Quote
{
    public required string Id { get; init; }
    public required string ClientId { get; init; }
    public required string ClientName { get; init; }
    public required string TargetType { get; init; }
    public required string TargetId { get; init; }
    public required string TargetName { get; init; }
    public string? EventId { get; init; }
    public required string EventType { get; init; }
    public required string EventDate { get; init; }
    public required int Guests { get; init; }
    public required string[] RequestedServices { get; init; }
    public string? Message { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
