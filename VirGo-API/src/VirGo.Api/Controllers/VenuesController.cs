using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirGo.Application.Marketplace;

namespace VirGo.Api.Controllers;

[ApiController]
[Route("api/v1/venues")]
public sealed class VenuesController(IVenueRepository venues) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] string? zone, [FromQuery] string? category, CancellationToken cancellationToken)
    {
        var result = await venues.ListPublishedAsync(zone, category, cancellationToken);
        return Ok(new
        {
            data = result.Select(venue => new
            {
                id = venue.Id,
                name = venue.Name,
                shortName = venue.ShortName,
                description = venue.Description,
                address = venue.Address,
                zone = venue.Zone,
                capacity = venue.Capacity,
                price = new { baseAmount = venue.BasePrice, unit = venue.PriceUnit },
                categories = venue.Categories,
                amenities = venue.Amenities,
                photos = venue.Photos,
                rating = new { average = venue.RatingAverage, count = venue.RatingCount }
            })
        });
    }
}
