using AstroTrack.Api.DTOs.Observations;
using AstroTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AstroTrack.Api.Controllers;

[ApiController]
[Route("api/observations")]
public class ObservationsController : ControllerBase
{
    private readonly IObservationService _observationService;

    public ObservationsController(IObservationService observationService)
    {
        _observationService = observationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var observations = await _observationService.GetAllAsync();
        return Ok(observations);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var observation = await _observationService.GetByIdAsync(id);
        if (observation is null)
        {
            return NotFound();
        }

        return Ok(observation);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateObservationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _observationService.CreateAsync(dto);
        return result.Status switch
        {
            ObservationMutationStatus.Success => CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.ObservationId },
                result.Data),
            ObservationMutationStatus.Duplicate => Conflict(new { message = result.Message }),
            ObservationMutationStatus.ValidationFailed => BadRequest(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to create observation." })
        };
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateObservationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _observationService.UpdateAsync(id, dto);
        return result.Status switch
        {
            ObservationMutationStatus.Success => Ok(result.Data),
            ObservationMutationStatus.NotFound => NotFound(),
            ObservationMutationStatus.ValidationFailed => BadRequest(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to update observation." })
        };
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _observationService.DeleteAsync(id);
        return result.Status switch
        {
            ObservationMutationStatus.Success => NoContent(),
            ObservationMutationStatus.NotFound => NotFound(),
            ObservationMutationStatus.ValidationFailed => Conflict(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to delete observation." })
        };
    }
}
