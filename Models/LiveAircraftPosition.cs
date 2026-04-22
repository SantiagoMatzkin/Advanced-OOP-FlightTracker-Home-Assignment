namespace FlightTracker.Models;

public sealed class LiveAircraftPosition
{
    public required string Icao24 { get; init; }
    public string CallSign { get; init; } = string.Empty;
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public double? VelocityMetersPerSecond { get; init; }
    public DateTime LastContactUtc { get; init; }
}
