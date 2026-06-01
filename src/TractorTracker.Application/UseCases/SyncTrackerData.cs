using NetTopologySuite.Geometries;
using TractorTracker.Application.Interfaces;
using TractorTracker.Domain.Entities;
using TractorTracker.Domain.Repositories;
using TractorTracker.Domain.Services;

namespace TractorTracker.Application.UseCases;

public class SyncTrackerData(
    IMachineRepository machines,
    IPositionRepository positions,
    ITrackerProviderService tracker) : ITrackerSyncService
{
    private static readonly GeometryFactory GeoFactory = new(new PrecisionModel(), 4326);

    public async Task SyncMachineAsync(Guid machineId, CancellationToken ct = default)
    {
        var machine = await machines.GetByIdAsync(machineId, ct)
            ?? throw new InvalidOperationException($"Machine {machineId} not found.");

        var since = await positions.GetLastRecordedAtAsync(machineId, ct)
            ?? DateTimeOffset.UtcNow.AddDays(-7);

        var newPositions = await tracker.GetPositionsSinceAsync(machine.TrackerDeviceId, since, ct);
        if (newPositions.Count == 0) return;

        var records = newPositions
            .Select(p => new PositionRecord(
                machineId,
                GeoFactory.CreatePoint(new Coordinate(p.Longitude, p.Latitude)),
                p.RecordedAt,
                p.SpeedKmh,
                p.HeadingDegrees))
            .ToList();

        await positions.AddRangeAsync(records, ct);
        machine.UpdateLastSync(DateTimeOffset.UtcNow);
        await positions.SaveChangesAsync(ct);
        await machines.SaveChangesAsync(ct);
    }
}
