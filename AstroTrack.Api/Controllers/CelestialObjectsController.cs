using AstroTrack.Api.DTOs.CelestialObjects;
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCelestialObjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _celestialObjectService.CreateAsync(dto);
        return result.Status switch
        {
            CelestialObjectMutationStatus.Success => CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.ObjectId },
                result.Data),
            CelestialObjectMutationStatus.Duplicate => Conflict(new { message = result.Message }),
            CelestialObjectMutationStatus.ValidationFailed => BadRequest(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to create celestial object." })
        };
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCelestialObjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _celestialObjectService.UpdateAsync(id, dto);
        return result.Status switch
        {
            CelestialObjectMutationStatus.Success => Ok(result.Data),
            CelestialObjectMutationStatus.NotFound => NotFound(),
            CelestialObjectMutationStatus.ValidationFailed => BadRequest(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to update celestial object." })
        };
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _celestialObjectService.DeleteAsync(id);
        return result.Status switch
        {
            CelestialObjectMutationStatus.Success => NoContent(),
            CelestialObjectMutationStatus.NotFound => NotFound(),
            CelestialObjectMutationStatus.ValidationFailed => Conflict(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to delete celestial object." })
        };
    }
}
