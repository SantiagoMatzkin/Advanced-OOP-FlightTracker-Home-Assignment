namespace FlightTracker.Models;

public sealed class RouteDestinationSummary
{
    public required string Route { get; init; }
    public required string DestinationName { get; init; }
    public required string DestinationCountry { get; init; }
    public required double DestinationLatitude { get; init; }
    public required double DestinationLongitude { get; init; }
    public required int FlightsCount { get; init; }
}
