using System.Globalization;
using VirGo.Application.Abstractions;
using VirGo.Domain.Marketplace;

namespace VirGo.Application.Marketplace;

public sealed class QuoteHandler(IVenueRepository venues, IQuoteRepository quotes, IClock clock)
{
    public async Task<QuoteCreationResult> HandleAsync(CreateQuoteCommand command, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(command.TargetType, "venue", StringComparison.OrdinalIgnoreCase)
            || !Guid.TryParse(command.TargetId, out _)
            || command.Guests < 1
            || !AllowedEventTypes.Contains(command.EventType, StringComparer.Ordinal)
            || command.RequestedServices.Any(service => string.IsNullOrWhiteSpace(service) || service.Length > 100)
            || command.Message is not null && command.Message.Length > 2000
            || command.EventDate < DateOnly.FromDateTime(clock.UtcNow))
        {
            return new(null, true);
        }

        var venue = await venues.FindPublishedAsync(command.TargetId, cancellationToken);
        if (venue is null)
        {
            return new(null, false);
        }

        var now = clock.UtcNow;
        var quote = new Quote
        {
            Id = Guid.NewGuid().ToString("D"),
            ClientId = command.ClientId,
            ClientName = command.ClientName,
            TargetType = "venue",
            TargetId = venue.Id,
            TargetName = venue.Name,
            EventId = string.IsNullOrWhiteSpace(command.EventId) ? null : command.EventId,
            EventType = command.EventType.Trim(),
            EventDate = command.EventDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Guests = command.Guests,
            RequestedServices = command.RequestedServices,
            Message = command.Message,
            Status = "sent",
            CreatedAt = now,
            UpdatedAt = now
        };
        await quotes.AddAsync(quote, cancellationToken);
        return new(quote, false);
    }

    private static readonly string[] AllowedEventTypes = ["Boda", "XV años", "Cumpleaños", "Corporativo", "Otro"];
}
