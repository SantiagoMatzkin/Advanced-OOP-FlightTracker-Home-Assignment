namespace FlightTracker.Models;

public sealed class UserPreferences
{
    public int SelectedTabIndex { get; set; }
    public string AirportSearchText { get; set; } = string.Empty;
    public DateTimeOffset? DepartureDate { get; set; }
    public string AircraftFilter { get; set; } = "All aircraft";
    public string StatusFilter { get; set; } = "All statuses";
    public string DateFilter { get; set; } = "All dates";
    public bool ShowStatusChart { get; set; }
    public bool ShowAircraftChart { get; set; }
    public bool ShowHourlyChart { get; set; }
    public bool LiveTrackingEnabled { get; set; }
}
