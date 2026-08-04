using AstroTrack.Api.DTOs.Researchers;
using AstroTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AstroTrack.Api.Controllers;

[ApiController]
[Route("api/researchers")]
public class ResearchersController : ControllerBase
{
    private readonly IResearcherService _researcherService;

    public ResearchersController(IResearcherService researcherService)
    {
        _researcherService = researcherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var researchers = await _researcherService.GetAllAsync();
        return Ok(researchers);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var researcher = await _researcherService.GetByIdAsync(id);
        if (researcher is null)
        {
            return NotFound();
        }

        return Ok(researcher);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResearcherDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _researcherService.CreateAsync(dto);
        return result.Status switch
        {
            ResearcherMutationStatus.Success => CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.ResearcherId },
                result.Data),
            ResearcherMutationStatus.Duplicate => Conflict(new { message = result.Message }),
            ResearcherMutationStatus.ValidationFailed => BadRequest(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to create researcher." })
        };
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateResearcherDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _researcherService.UpdateAsync(id, dto);
        return result.Status switch
        {
            ResearcherMutationStatus.Success => Ok(result.Data),
            ResearcherMutationStatus.NotFound => NotFound(),
            ResearcherMutationStatus.ValidationFailed => BadRequest(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to update researcher." })
        };
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _researcherService.DeleteAsync(id);
        return result.Status switch
        {
            ResearcherMutationStatus.Success => NoContent(),
            ResearcherMutationStatus.NotFound => NotFound(),
            ResearcherMutationStatus.ValidationFailed => Conflict(new { message = result.Message }),
            _ => BadRequest(new { message = "Unable to delete researcher." })
        };
    }
}