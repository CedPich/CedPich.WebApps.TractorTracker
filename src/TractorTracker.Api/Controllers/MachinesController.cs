using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TractorTracker.Application.DTOs;
using TractorTracker.Application.UseCases;

namespace TractorTracker.Api.Controllers;

[ApiController]
[Route("api/machine")]
public class MachinesController(
    GetCurrentPosition getCurrentPosition,
    GetPositionHistory getPositionHistory,
    GetDailyWorkHours getDailyWorkHours,
    IOptions<AppOptions> options) : ControllerBase
{
    private Guid MachineId => options.Value.MachineId;

    [HttpGet("position")]
    public async Task<ActionResult<PositionDto>> GetCurrentPosition(CancellationToken ct)
    {
        var result = await getCurrentPosition.ExecuteAsync(MachineId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<PositionDto>>> GetHistory(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken ct)
    {
        if (from >= to) return BadRequest("'from' must be before 'to'.");
        var result = await getPositionHistory.ExecuteAsync(MachineId, from, to, ct);
        return Ok(result);
    }

    [HttpGet("work-hours")]
    public async Task<ActionResult<IReadOnlyList<DailyWorkHoursDto>>> GetWorkHours(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        if (from > to) return BadRequest("'from' must be before or equal to 'to'.");
        var result = await getDailyWorkHours.ExecuteAsync(MachineId, from, to, ct);
        return Ok(result);
    }
}
