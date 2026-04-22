using CommunityToolkit.Mvvm.ComponentModel;

namespace FlightTracker.Models;

public sealed partial class FlightSearchFilters : ObservableObject
{
    [ObservableProperty]
    private string _airportSearchText = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _departureDate;

    [ObservableProperty]
    private string _aircraftFilter = "All aircraft";

    [ObservableProperty]
    private string _statusFilter = "All statuses";

    public void Load(UserPreferences preferences)
    {
        AirportSearchText = preferences.AirportSearchText;
        DepartureDate = preferences.DepartureDate;
        AircraftFilter = preferences.AircraftFilter;
        StatusFilter = preferences.StatusFilter;
    }

    public void Save(UserPreferences preferences)
    {
        preferences.AirportSearchText = AirportSearchText;
        preferences.DepartureDate = DepartureDate;
        preferences.AircraftFilter = AircraftFilter;
        preferences.StatusFilter = StatusFilter;
    }

    public bool MatchesFlight(Flight flight)
    {
        if (DepartureDate.HasValue && flight.DepartureUtc.Date != DepartureDate.Value.Date)
        {
            return false;
        }

        if (!string.Equals(AircraftFilter, "All aircraft", StringComparison.OrdinalIgnoreCase) &&
            !flight.AircraftType.Contains(AircraftFilter, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(StatusFilter, "All statuses", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(flight.Status, StatusFilter, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}