using NetTopologySuite.Geometries;

namespace TractorTracker.Domain.Entities;

public class PositionRecord
{
    public long Id { get; private set; }
    public Guid MachineId { get; private set; }
    public Point Location { get; private set; } = null!;
    public double? SpeedKmh { get; private set; }
    public double? HeadingDegrees { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }

    private PositionRecord() { }

    public PositionRecord(Guid machineId, Point location, DateTimeOffset recordedAt, double? speedKmh = null, double? headingDegrees = null)
    {
        MachineId = machineId;
        Location = location;
        RecordedAt = recordedAt;
        SpeedKmh = speedKmh;
        HeadingDegrees = headingDegrees;
    }

    public double Latitude => Location.Y;
    public double Longitude => Location.X;
}
