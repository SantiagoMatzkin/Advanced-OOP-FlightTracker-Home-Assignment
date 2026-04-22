using System.Text.Json.Serialization;

namespace FlightTracker.Models;

public sealed class Flight
{
    [JsonPropertyName("FlightNumber")]
    public string FlightNumber { get; set; } = string.Empty;

    [JsonPropertyName("Airline")]
    public string Airline { get; set; } = string.Empty;

    [JsonPropertyName("From")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("FromName")]
    public string FromName { get; set; } = string.Empty;

    [JsonPropertyName("FromCountry")]
    public string FromCountry { get; set; } = string.Empty;

    [JsonPropertyName("FromLatitude")]
    public double FromLatitude { get; set; }

    [JsonPropertyName("FromLongitude")]
    public double FromLongitude { get; set; }

    [JsonPropertyName("To")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("ToName")]
    public string ToName { get; set; } = string.Empty;

    [JsonPropertyName("ToCountry")]
    public string ToCountry { get; set; } = string.Empty;

    [JsonPropertyName("ToLatitude")]
    public double ToLatitude { get; set; }

    [JsonPropertyName("ToLongitude")]
    public double ToLongitude { get; set; }

    [JsonPropertyName("DepartureUtc")]
    public DateTime DepartureUtc { get; set; }

    [JsonPropertyName("AircraftType")]
    public string AircraftType { get; set; } = string.Empty;

    [JsonPropertyName("Status")]
    public string Status { get; set; } = "Scheduled";

    public string OriginDisplay => $"{FromName} ({From})";
    public string DestinationDisplay => $"{ToName} ({To})";
}
