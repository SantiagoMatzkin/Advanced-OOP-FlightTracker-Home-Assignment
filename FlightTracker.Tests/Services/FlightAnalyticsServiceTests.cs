using FlightTracker.Services;
using FlightTracker.Tests.TestData;
using Xunit;

namespace FlightTracker.Tests.Services;

public class FlightAnalyticsServiceTests
{
    [Fact]
    public void GetTopAirlines_ReturnsDescendingCounts()
    {
        var service = new FlightAnalyticsService();
        var result = service.GetTopAirlines(SampleFlights.Build(), top: 2);

        Assert.Equal(2, result.Count);
        Assert.Equal("KLM", result[0].Airline);
        Assert.Equal(2, result[0].FlightsCount);
        Assert.Equal("SAS", result[1].Airline);
        Assert.Equal(2, result[1].FlightsCount);
    }

    [Fact]
    public void GetBusiestRoutesByTimeOfDay_ReturnsOnePerBucket()
    {
        var service = new FlightAnalyticsService();
        var result = service.GetBusiestRoutesByTimeOfDay(SampleFlights.Build());

        Assert.Contains(result, metric => metric.TimeOfDay == "Night");
        Assert.Contains(result, metric => metric.TimeOfDay == "Morning");
        Assert.Contains(result, metric => metric.TimeOfDay == "Afternoon");
        Assert.Contains(result, metric => metric.TimeOfDay == "Evening");
    }

    [Fact]
    public void GetStatusAndAircraftBreakdowns_ReturnOrderedMetrics()
    {
        var service = new FlightAnalyticsService();

        var statusBreakdown = service.GetStatusBreakdown(SampleFlights.Build());
        var aircraftUsage = service.GetAircraftUsage(SampleFlights.Build());
        var departureHours = service.GetDepartureHours(SampleFlights.Build());

        Assert.Contains(statusBreakdown, metric => metric.Status == "Scheduled");
        Assert.Contains(aircraftUsage, metric => metric.AircraftType == "A320");
        Assert.Contains(departureHours, metric => metric.Hour == 7);
    }
}
