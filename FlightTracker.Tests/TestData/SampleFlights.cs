using FlightTracker.Models;

namespace FlightTracker.Tests.TestData;

internal static class SampleFlights
{
    public static IReadOnlyList<Flight> Build() =>
    [
        new Flight
        {
            FlightNumber = "SK001",
            Airline = "SAS",
            From = "CPH",
            FromName = "Copenhagen Airport",
            FromCountry = "Denmark",
            FromLatitude = 55.618,
            FromLongitude = 12.656,
            To = "LHR",
            ToName = "London Heathrow Airport",
            ToCountry = "United Kingdom",
            ToLatitude = 51.47,
            ToLongitude = -0.4543,
            DepartureUtc = new DateTime(2026, 4, 22, 7, 0, 0, DateTimeKind.Utc),
            AircraftType = "A320",
            Status = "Scheduled"
        },
        new Flight
        {
            FlightNumber = "SK002",
            Airline = "SAS",
            From = "CPH",
            FromName = "Copenhagen Airport",
            FromCountry = "Denmark",
            FromLatitude = 55.618,
            FromLongitude = 12.656,
            To = "FRA",
            ToName = "Frankfurt Airport",
            ToCountry = "Germany",
            ToLatitude = 50.0379,
            ToLongitude = 8.5622,
            DepartureUtc = new DateTime(2026, 4, 22, 13, 0, 0, DateTimeKind.Utc),
            AircraftType = "A320",
            Status = "Delayed"
        },
        new Flight
        {
            FlightNumber = "LH100",
            Airline = "Lufthansa",
            From = "FRA",
            FromName = "Frankfurt Airport",
            FromCountry = "Germany",
            FromLatitude = 50.0379,
            FromLongitude = 8.5622,
            To = "CPH",
            ToName = "Copenhagen Airport",
            ToCountry = "Denmark",
            ToLatitude = 55.618,
            ToLongitude = 12.656,
            DepartureUtc = new DateTime(2026, 4, 22, 19, 0, 0, DateTimeKind.Utc),
            AircraftType = "A321",
            Status = "Scheduled"
        },
        new Flight
        {
            FlightNumber = "KL010",
            Airline = "KLM",
            From = "AMS",
            FromName = "Amsterdam Schiphol Airport",
            FromCountry = "Netherlands",
            FromLatitude = 52.3105,
            FromLongitude = 4.7683,
            To = "CPH",
            ToName = "Copenhagen Airport",
            ToCountry = "Denmark",
            ToLatitude = 55.618,
            ToLongitude = 12.656,
            DepartureUtc = new DateTime(2026, 4, 22, 2, 0, 0, DateTimeKind.Utc),
            AircraftType = "B737",
            Status = "Boarding"
        },
        new Flight
        {
            FlightNumber = "KL011",
            Airline = "KLM",
            From = "AMS",
            FromName = "Amsterdam Schiphol Airport",
            FromCountry = "Netherlands",
            FromLatitude = 52.3105,
            FromLongitude = 4.7683,
            To = "LHR",
            ToName = "London Heathrow Airport",
            ToCountry = "United Kingdom",
            ToLatitude = 51.47,
            ToLongitude = -0.4543,
            DepartureUtc = new DateTime(2026, 4, 22, 9, 30, 0, DateTimeKind.Utc),
            AircraftType = "B737",
            Status = "Scheduled"
        }
    ];
}
