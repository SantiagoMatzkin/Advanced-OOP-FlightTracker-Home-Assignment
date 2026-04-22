using FlightTracker.Services;
using FlightTracker.Tests.TestData;
using FlightTracker.ViewModels;
using Xunit;

namespace FlightTracker.Tests.ViewModels;

public class AirportFlightInfoViewModelTests
{
    [Fact]
    public void SelectingAirportAndStatus_UpdatesFilteredFlights()
    {
        var exportService = new ExportService(Path.Combine(Path.GetTempPath(), "FlightTrackerTests"));
        var filters = new FlightTracker.Models.FlightSearchFilters();
        var viewModel = new AirportFlightInfoViewModel(SampleFlights.Build(), filters, exportService);

        viewModel.SelectedAirport = viewModel.Airports.Single(airport => airport.Code == "CPH");
        filters.StatusFilter = "Delayed";

        Assert.Single(viewModel.FilteredFlights);
        Assert.Equal("SK002", viewModel.FilteredFlights[0].FlightNumber);
    }
}
