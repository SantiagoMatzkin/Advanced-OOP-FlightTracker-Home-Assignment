namespace FlightTracker.Models;

public sealed record AirlineTrafficMetric(string Airline, int FlightsCount);

public sealed record TimeOfDayRouteMetric(string TimeOfDay, string Route, int FlightsCount);

public sealed record CountryTrafficMetric(string Country, int FlightsCount);

public sealed record FlightStatusMetric(string Status, int FlightsCount);

public sealed record AircraftUsageMetric(string AircraftType, int FlightsCount);

public sealed record DepartureHourMetric(int Hour, int FlightsCount)
{
    public string HourLabel => $"{Hour:00}:00";
}

public sealed class AnalyticsExportSnapshot
{
    public required DateTime GeneratedUtc { get; init; }
    public required IReadOnlyList<AirlineTrafficMetric> TopAirlines { get; init; }
    public required IReadOnlyList<TimeOfDayRouteMetric> BusiestRoutesByTimeOfDay { get; init; }
    public required IReadOnlyList<CountryTrafficMetric> CountryTraffic { get; init; }
    public required IReadOnlyList<FlightStatusMetric> FlightStatusBreakdown { get; init; }
    public required IReadOnlyList<AircraftUsageMetric> AircraftUsage { get; init; }
    public required IReadOnlyList<DepartureHourMetric> DepartureHours { get; init; }
}
