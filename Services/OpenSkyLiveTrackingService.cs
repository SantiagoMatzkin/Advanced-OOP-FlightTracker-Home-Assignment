using System.Text.Json;
using FlightTracker.Models;

namespace FlightTracker.Services;

public interface ILiveAircraftTrackingService
{
    Task<IReadOnlyList<LiveAircraftPosition>> GetLiveAircraftPositionsAsync(CancellationToken cancellationToken = default);
}

public sealed class OpenSkyLiveTrackingService : ILiveAircraftTrackingService
{
    private static readonly HttpClient HttpClient = new();

    public async Task<IReadOnlyList<LiveAircraftPosition>> GetLiveAircraftPositionsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await HttpClient.GetAsync("https://opensky-network.org/api/states/all", cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("states", out var statesElement) || statesElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var positions = new List<LiveAircraftPosition>();
        foreach (var state in statesElement.EnumerateArray())
        {
            if (state.ValueKind != JsonValueKind.Array || state.GetArrayLength() < 11)
            {
                continue;
            }

            var latitude = state[6].ValueKind == JsonValueKind.Number ? state[6].GetDouble() : double.NaN;
            var longitude = state[5].ValueKind == JsonValueKind.Number ? state[5].GetDouble() : double.NaN;
            if (double.IsNaN(latitude) || double.IsNaN(longitude))
            {
                continue;
            }

            var icao24 = state[0].GetString();
            if (string.IsNullOrWhiteSpace(icao24))
            {
                continue;
            }

            var callSign = state[1].GetString()?.Trim() ?? string.Empty;
            double? velocity = state[9].ValueKind == JsonValueKind.Number ? state[9].GetDouble() : null;
            var lastContactUtc = state[4].ValueKind == JsonValueKind.Number
                ? DateTimeOffset.FromUnixTimeSeconds(state[4].GetInt64()).UtcDateTime
                : DateTime.UtcNow;

            positions.Add(new LiveAircraftPosition
            {
                Icao24 = icao24,
                CallSign = callSign,
                Latitude = latitude,
                Longitude = longitude,
                VelocityMetersPerSecond = velocity,
                LastContactUtc = lastContactUtc
            });
        }

        return positions;
    }
}