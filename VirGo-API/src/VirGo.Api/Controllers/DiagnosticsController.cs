using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VirGo.Api.Controllers;

[ApiController]
[Route("api/v1/_diagnostics")]
public sealed class DiagnosticsController(IHostEnvironment environment) : ControllerBase
{
    [HttpGet("secure")]
    [Authorize]
    public IActionResult Secure()
    {
        return environment.IsEnvironment("Testing")
            ? Ok(new { status = "Authorized" })
            : NotFound();
    }

    [HttpGet("failure")]
    [AllowAnonymous]
    public IActionResult Failure()
    {
        if (!environment.IsEnvironment("Testing"))
        {
            return NotFound();
        }

        throw new InvalidOperationException("Synthetic failure for integration tests.");
    }
}
