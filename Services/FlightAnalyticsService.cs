using FlightTracker.Models;

namespace FlightTracker.Services;

public interface IFlightAnalyticsService
{
    IReadOnlyList<AirlineTrafficMetric> GetTopAirlines(IEnumerable<Flight> flights, int top = 5);
    IReadOnlyList<TimeOfDayRouteMetric> GetBusiestRoutesByTimeOfDay(IEnumerable<Flight> flights);
    IReadOnlyList<CountryTrafficMetric> GetCountryTraffic(IEnumerable<Flight> flights);
    IReadOnlyList<FlightStatusMetric> GetStatusBreakdown(IEnumerable<Flight> flights);
    IReadOnlyList<AircraftUsageMetric> GetAircraftUsage(IEnumerable<Flight> flights);
    IReadOnlyList<DepartureHourMetric> GetDepartureHours(IEnumerable<Flight> flights);
    AnalyticsExportSnapshot BuildExportSnapshot(IEnumerable<Flight> flights);
}

public sealed class FlightAnalyticsService : IFlightAnalyticsService
{
    public IReadOnlyList<AirlineTrafficMetric> GetTopAirlines(IEnumerable<Flight> flights, int top = 5)
    {
        return flights
            .GroupBy(flight => flight.Airline)
            .Select(group => new AirlineTrafficMetric(group.Key, group.Count()))
            .OrderByDescending(metric => metric.FlightsCount)
            .ThenBy(metric => metric.Airline)
            .Take(top)
            .ToList();
    }

    public IReadOnlyList<TimeOfDayRouteMetric> GetBusiestRoutesByTimeOfDay(IEnumerable<Flight> flights)
    {
        return flights
            .GroupBy(flight => GetTimeOfDayBucket(flight.DepartureUtc))
            .Select(bucket => bucket
                .GroupBy(flight => $"{flight.From} -> {flight.To}")
                .Select(group => new TimeOfDayRouteMetric(bucket.Key, group.Key, group.Count()))
                .OrderByDescending(metric => metric.FlightsCount)
                .ThenBy(metric => metric.Route)
                .First())
            .OrderBy(metric => TimeOfDayOrder(metric.TimeOfDay))
            .ToList();
    }

    public IReadOnlyList<CountryTrafficMetric> GetCountryTraffic(IEnumerable<Flight> flights)
    {
        return flights
            .GroupBy(flight => flight.ToCountry)
            .Select(group => new CountryTrafficMetric(group.Key, group.Count()))
            .OrderByDescending(metric => metric.FlightsCount)
            .ThenBy(metric => metric.Country)
            .ToList();
    }

    public IReadOnlyList<FlightStatusMetric> GetStatusBreakdown(IEnumerable<Flight> flights)
    {
        return flights
            .GroupBy(flight => flight.Status)
            .Select(group => new FlightStatusMetric(group.Key, group.Count()))
            .OrderByDescending(metric => metric.FlightsCount)
            .ThenBy(metric => metric.Status)
            .ToList();
    }

    public IReadOnlyList<AircraftUsageMetric> GetAircraftUsage(IEnumerable<Flight> flights)
    {
        return flights
            .GroupBy(flight => flight.AircraftType)
            .Select(group => new AircraftUsageMetric(group.Key, group.Count()))
            .OrderByDescending(metric => metric.FlightsCount)
            .ThenBy(metric => metric.AircraftType)
            .ToList();
    }

    public IReadOnlyList<DepartureHourMetric> GetDepartureHours(IEnumerable<Flight> flights)
    {
        return flights
            .GroupBy(flight => flight.DepartureUtc.Hour)
            .Select(group => new DepartureHourMetric(group.Key, group.Count()))
            .OrderBy(metric => metric.Hour)
            .ToList();
    }

    public AnalyticsExportSnapshot BuildExportSnapshot(IEnumerable<Flight> flights)
    {
        var materializedFlights = flights.ToList();

        return new AnalyticsExportSnapshot
        {
            GeneratedUtc = DateTime.UtcNow,
            TopAirlines = GetTopAirlines(materializedFlights, 10),
            BusiestRoutesByTimeOfDay = GetBusiestRoutesByTimeOfDay(materializedFlights),
            CountryTraffic = GetCountryTraffic(materializedFlights),
            FlightStatusBreakdown = GetStatusBreakdown(materializedFlights),
            AircraftUsage = GetAircraftUsage(materializedFlights),
            DepartureHours = GetDepartureHours(materializedFlights)
        };
    }

    private static string GetTimeOfDayBucket(DateTime departureUtc)
    {
        var hour = departureUtc.Hour;
        return hour switch
        {
            >= 6 and < 12 => "Morning",
            >= 12 and < 18 => "Afternoon",
            >= 18 and < 24 => "Evening",
            _ => "Night"
        };
    }

    private static int TimeOfDayOrder(string timeOfDay)
    {
        return timeOfDay switch
        {
            "Morning" => 0,
            "Afternoon" => 1,
            "Evening" => 2,
            _ => 3
        };
    }
}
