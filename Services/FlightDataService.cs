using System.Text.Json;
using FlightTracker.Models;

namespace FlightTracker.Services;

public interface IFlightDataService
{
    IReadOnlyList<Flight> LoadFlights();
}

public sealed class JsonFlightDataService : IFlightDataService
{
    private readonly string _jsonPath;

    public JsonFlightDataService(string? jsonPath = null)
    {
        _jsonPath = jsonPath ?? Path.Combine(AppContext.BaseDirectory, "Assets", "flights.json");
    }

    public IReadOnlyList<Flight> LoadFlights()
    {
        if (!File.Exists(_jsonPath))
        {
            throw new FileNotFoundException($"Flight data file was not found: {_jsonPath}");
        }

        var json = File.ReadAllText(_jsonPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var flights = JsonSerializer.Deserialize<List<Flight>>(json, options)
                      ?? throw new InvalidOperationException("Unable to deserialize flights.json.");

        if (flights.Count == 0)
        {
            throw new InvalidOperationException("No flight records were found in flights.json.");
        }

        foreach (var flight in flights)
        {
            NormalizeAndValidateFlight(flight);
        }

        return flights;
    }

    private static void NormalizeAndValidateFlight(Flight flight)
    {
        flight.From = RequireValue(flight.From, "From");
        flight.To = RequireValue(flight.To, "To");
        flight.FlightNumber = RequireValue(flight.FlightNumber, "FlightNumber");
        flight.Airline = RequireValue(flight.Airline, "Airline");
        flight.AircraftType = RequireValue(flight.AircraftType, "AircraftType");

        if (flight.DepartureUtc == default)
        {
            throw new InvalidOperationException($"Flight '{flight.FlightNumber}' has an invalid DepartureUtc value.");
        }

        flight.FromName = string.IsNullOrWhiteSpace(flight.FromName) ? flight.From : flight.FromName.Trim();
        flight.ToName = string.IsNullOrWhiteSpace(flight.ToName) ? flight.To : flight.ToName.Trim();
        flight.FromCountry = string.IsNullOrWhiteSpace(flight.FromCountry) ? "Unknown" : flight.FromCountry.Trim();
        flight.ToCountry = string.IsNullOrWhiteSpace(flight.ToCountry) ? "Unknown" : flight.ToCountry.Trim();
        flight.Status = string.IsNullOrWhiteSpace(flight.Status) ? "Scheduled" : flight.Status.Trim();
    }

    private static string RequireValue(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"A required flight field is missing: {fieldName}");
        }

        return value.Trim();
    }
}
