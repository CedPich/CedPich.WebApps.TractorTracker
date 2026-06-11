namespace TractorTracker.Application.DTOs;

public record PositionDto(
    double Latitude,
    double Longitude,
    DateTimeOffset RecordedAt,
    double? SpeedKmh,
    double? HeadingDegrees,
    double? AltitudeMeters,
    int? Satellites,
    string? FormattedAddress);

public record DailyWorkHoursDto(
    DateOnly Date,
    double Hours,
    int PositionCount);
