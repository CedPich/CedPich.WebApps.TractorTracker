namespace TractorTracker.Application.DTOs;

public record PositionDto(
    double Latitude,
    double Longitude,
    DateTimeOffset RecordedAt,
    double? SpeedKmh,
    double? HeadingDegrees);

public record DailyWorkHoursDto(
    DateOnly Date,
    double Hours,
    int PositionCount);
