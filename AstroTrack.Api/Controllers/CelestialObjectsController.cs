using AstroTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AstroTrack.Api.Controllers;

[ApiController]
[Route("api/celestial-objects")]
public class CelestialObjectsController : ControllerBase
{
    private readonly ICelestialObjectService _celestialObjectService;

    public CelestialObjectsController(ICelestialObjectService celestialObjectService)
    {
        _celestialObjectService = celestialObjectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var celestialObjects = await _celestialObjectService.GetAllAsync();
        return Ok(celestialObjects);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var celestialObject = await _celestialObjectService.GetByIdAsync(id);
        if (celestialObject is null)
        {
            return NotFound();
        }

        return Ok(celestialObject);
    }
}
