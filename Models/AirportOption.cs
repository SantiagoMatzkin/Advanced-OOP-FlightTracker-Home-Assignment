namespace FlightTracker.Models;

public sealed class AirportOption
{
    public AirportOption(string code, string name, string country, double latitude, double longitude)
    {
        Code = code;
        Name = name;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string Code { get; }
    public string Name { get; }
    public string Country { get; }
    public double Latitude { get; }
    public double Longitude { get; }
    public string DisplayName => $"{Name} ({Code})";

    public override string ToString() => DisplayName;
}
