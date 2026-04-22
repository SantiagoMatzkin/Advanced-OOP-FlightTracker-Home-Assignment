using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlightTracker.Models;
using FlightTracker.Services;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace FlightTracker.ViewModels;

public sealed partial class AirportFlightInfoViewModel : ViewModelBase
{
    private readonly IReadOnlyList<Flight> _flights;
    private readonly FlightSearchFilters _filters;
    private readonly IExportService _exportService;

    [ObservableProperty]
    private AirportOption? _selectedAirport;

    [ObservableProperty]
    private string _exportMessage = "Select an airport and status filter to inspect flights.";

    public AirportFlightInfoViewModel(IReadOnlyList<Flight> flights, FlightSearchFilters filters, IExportService exportService)
    {
        _flights = flights;
        _filters = filters;
        _exportService = exportService;

        Airports = _flights
            .GroupBy(flight => new
            {
                flight.From,
                flight.FromName,
                flight.FromCountry,
                flight.FromLatitude,
                flight.FromLongitude
            })
            .Select(group => new AirportOption(
                group.Key.From,
                group.Key.FromName,
                group.Key.FromCountry,
                group.Key.FromLatitude,
                group.Key.FromLongitude))
            .OrderBy(airport => airport.DisplayName)
            .ToList();

        AircraftFilters = new[] { "All aircraft" }
            .Concat(_flights.Select(flight => flight.AircraftType).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value))
            .ToList();

        StatusFilters = new[] { "All statuses" }
            .Concat(_flights.Select(flight => flight.Status).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(status => status))
            .ToList();

        _filters.PropertyChanged += OnFiltersChanged;
        RefreshFilteredFlights();
    }

    public FlightSearchFilters Filters => _filters;
    public IReadOnlyList<AirportOption> Airports { get; }
    public IReadOnlyList<string> AircraftFilters { get; }
    public IReadOnlyList<string> StatusFilters { get; }
    public ObservableCollection<Flight> FilteredFlights { get; } = new();

    [RelayCommand]
    private void ExportFlights()
    {
        if (FilteredFlights.Count == 0)
        {
            ExportMessage = "No rows to export. Select an airport and status with available flights.";
            return;
        }

        var exportPath = _exportService.ExportFlightsCsv(FilteredFlights);
        ExportMessage = $"Flights exported to: {exportPath}";
    }

    partial void OnSelectedAirportChanged(AirportOption? value) => RefreshFilteredFlights();

    private void OnFiltersChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FlightSearchFilters.DepartureDate)
            or nameof(FlightSearchFilters.AircraftFilter)
            or nameof(FlightSearchFilters.StatusFilter))
        {
            RefreshFilteredFlights();
        }
    }

    private void RefreshFilteredFlights()
    {
        FilteredFlights.Clear();

        if (SelectedAirport is null)
        {
            ExportMessage = "Select an airport to load flight details.";
            return;
        }

        var statusFilter = SelectedStatusFilter;
        var flights = _flights
            .Where(flight => flight.From == SelectedAirport.Code)
            .Where(Filters.MatchesFlight)
            .OrderBy(flight => flight.DepartureUtc);

        foreach (var flight in flights)
        {
            FilteredFlights.Add(flight);
        }

        ExportMessage = FilteredFlights.Count == 0
            ? "No flights match the selected airport/status combination."
            : $"Showing {FilteredFlights.Count} flight(s) for {SelectedAirport.DisplayName}.";
    }

    public string SelectedStatusFilter
    {
        get => Filters.StatusFilter;
        set => Filters.StatusFilter = value;
    }

    public string SelectedAircraftFilter
    {
        get => Filters.AircraftFilter;
        set => Filters.AircraftFilter = value;
    }

    public DateTimeOffset? SelectedDepartureDate
    {
        get => Filters.DepartureDate;
        set => Filters.DepartureDate = value;
    }
}
