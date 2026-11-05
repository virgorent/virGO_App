using VirGo.Domain.Marketplace;

namespace VirGo.Application.Marketplace;

public sealed class DuplicateBookingIdempotencyException : Exception
{
    public DuplicateBookingIdempotencyException() : base("The booking idempotency key already exists.")
    {
    }
}

public interface IVenueRepository
{
    Task<IReadOnlyList<Venue>> ListPublishedAsync(string? zone, string? category, CancellationToken cancellationToken = default);
    Task<Venue?> FindPublishedAsync(string id, CancellationToken cancellationToken = default);
}

public interface IQuoteRepository
{
    Task AddAsync(Quote quote, CancellationToken cancellationToken = default);
}

public interface IBookingRepository
{
    Task<Booking?> FindByIdempotencyKeyAsync(string clientId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<string> NextFolioAsync(int year, CancellationToken cancellationToken = default);
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
}
