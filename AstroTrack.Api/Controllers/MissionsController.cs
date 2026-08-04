using AstroTrack.Api.DTOs.Missions;
using AstroTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AstroTrack.Api.Controllers;

[ApiController]
[Route("api/missions")]
public class MissionsController : ControllerBase
{
    private readonly IMissionService _missionService;

    public MissionsController(IMissionService missionService)
    {
        _missionService = missionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var missions = await _missionService.GetAllAsync();
        return Ok(missions);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var mission = await _missionService.GetByIdAsync(id);
        if (mission is null)
        {
            return NotFound();
        }

        return Ok(mission);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _missionService.CreateAsync(dto);
        return result.Status switch
        {
            MissionMutationStatus.Success => CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.MissionId },
                result.Data),
            MissionMutationStatus.Duplicate => Conflict(new { message = result.Message }),
            MissionMutationStatus.ValidationFailed => BadRequest(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to create mission." })
        };
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateMissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _missionService.UpdateAsync(id, dto);
        return result.Status switch
        {
            MissionMutationStatus.Success => Ok(result.Data),
            MissionMutationStatus.NotFound => NotFound(),
            MissionMutationStatus.ValidationFailed => BadRequest(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to update mission." })
        };
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _missionService.DeleteAsync(id);
        return result.Status switch
        {
            MissionMutationStatus.Success => NoContent(),
            MissionMutationStatus.NotFound => NotFound(),
            MissionMutationStatus.ValidationFailed => Conflict(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to delete mission." })
        };
    }
}
