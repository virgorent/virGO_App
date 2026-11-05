namespace VirGo.Domain.Marketplace;

public sealed class Venue
{
    public required string Id { get; init; }
    public required string OwnerId { get; init; }
    public required string Name { get; init; }
    public required string ShortName { get; init; }
    public required string Description { get; init; }
    public required string Address { get; init; }
    public required string Zone { get; init; }
    public required int Capacity { get; init; }
    public required decimal BasePrice { get; init; }
    public required string PriceUnit { get; init; }
    public required int MinimumHours { get; init; }
    public required string[] Categories { get; init; }
    public required string[] Amenities { get; init; }
    public required string[] Photos { get; init; }
    public required string Status { get; init; }
    public required decimal RatingAverage { get; init; }
    public required int RatingCount { get; init; }
}
