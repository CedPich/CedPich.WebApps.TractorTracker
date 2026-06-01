using Microsoft.AspNetCore.Mvc;
using TractorTracker.Application.DTOs;
using TractorTracker.Application.Interfaces;
using TractorTracker.Application.UseCases;

namespace TractorTracker.Api.Controllers;

[ApiController]
[Route("api/machines")]
public class MachinesController(
    GetCurrentPosition getCurrentPosition,
    GetPositionHistory getPositionHistory,
    GetDailyWorkHours getDailyWorkHours,
    ITrackerSyncService syncService) : ControllerBase
{
    [HttpGet("{machineId:guid}/position")]
    public async Task<ActionResult<PositionDto>> GetCurrentPosition(Guid machineId, CancellationToken ct)
    {
        var result = await getCurrentPosition.ExecuteAsync(machineId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{machineId:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<PositionDto>>> GetHistory(
        Guid machineId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken ct)
    {
        if (from >= to) return BadRequest("'from' must be before 'to'.");
        var result = await getPositionHistory.ExecuteAsync(machineId, from, to, ct);
        return Ok(result);
    }

    [HttpGet("{machineId:guid}/work-hours")]
    public async Task<ActionResult<IReadOnlyList<DailyWorkHoursDto>>> GetWorkHours(
        Guid machineId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        if (from > to) return BadRequest("'from' must be before or equal to 'to'.");
        var result = await getDailyWorkHours.ExecuteAsync(machineId, from, to, ct);
        return Ok(result);
    }

    [HttpPost("{machineId:guid}/sync")]
    public async Task<IActionResult> Sync(Guid machineId, CancellationToken ct)
    {
        await syncService.SyncMachineAsync(machineId, ct);
        return NoContent();
    }
}
