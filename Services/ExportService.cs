using System.Text;
using System.Text.Json;
using FlightTracker.Models;

namespace FlightTracker.Services;

public interface IExportService
{
    string ExportFlightsCsv(IEnumerable<Flight> flights);
    string ExportAnalyticsJson(AnalyticsExportSnapshot snapshot);
}

public sealed class ExportService : IExportService
{
    private readonly string _exportDirectory;

    public ExportService(string? exportDirectory = null)
    {
        _exportDirectory = exportDirectory ?? Path.Combine(AppContext.BaseDirectory, "Exports");
    }

    public string ExportFlightsCsv(IEnumerable<Flight> flights)
    {
        Directory.CreateDirectory(_exportDirectory);
        var filePath = Path.Combine(_exportDirectory, $"flights-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");

        var builder = new StringBuilder();
        builder.AppendLine("FlightNumber,Airline,From,FromName,To,ToName,DepartureUtc,AircraftType,Status");

        foreach (var flight in flights)
        {
            builder.AppendLine(
                $"{EscapeCsv(flight.FlightNumber)}," +
                $"{EscapeCsv(flight.Airline)}," +
                $"{EscapeCsv(flight.From)}," +
                $"{EscapeCsv(flight.FromName)}," +
                $"{EscapeCsv(flight.To)}," +
                $"{EscapeCsv(flight.ToName)}," +
                $"{flight.DepartureUtc:O}," +
                $"{EscapeCsv(flight.AircraftType)}," +
                $"{EscapeCsv(flight.Status)}");
        }

        File.WriteAllText(filePath, builder.ToString());
        return filePath;
    }

    public string ExportAnalyticsJson(AnalyticsExportSnapshot snapshot)
    {
        Directory.CreateDirectory(_exportDirectory);
        var filePath = Path.Combine(_exportDirectory, $"analytics-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");

        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(filePath, json);
        return filePath;
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
