using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlightTracker.Models;
using FlightTracker.Services;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Color = Mapsui.Styles.Color;
using Point = NetTopologySuite.Geometries.Point;

namespace FlightTracker.ViewModels;

public sealed partial class RouteVisualizationViewModel : ViewModelBase
{
    private readonly IReadOnlyList<Flight> _flights;
    private readonly IReadOnlyList<AirportOption> _originAirports;
    private readonly FlightSearchFilters _filters;
    private readonly ILiveAircraftTrackingService _liveAircraftTrackingService;
    private readonly Action _persistPreferences;
    private readonly MemoryLayer _routesLayer = new("Routes");
    private readonly MemoryLayer _liveAircraftLayer = new("Live Aircraft");
    private readonly DispatcherTimer _liveRefreshTimer;
    private bool _isRefreshingLiveAircraft;

    [ObservableProperty]
    private AirportOption? _selectedOriginAirport;

    [ObservableProperty]
    private string _selectionHint = "Select an origin airport to display routes and map connections.";

    [ObservableProperty]
    private bool _liveTrackingEnabled;

    [ObservableProperty]
    private string _liveTrackingStatus = "Live tracking is off.";

    public RouteVisualizationViewModel(
        IReadOnlyList<Flight> flights,
        FlightSearchFilters filters,
        bool liveTrackingEnabled,
        Action persistPreferences)
    {
        _flights = flights;
        _filters = filters;
        _persistPreferences = persistPreferences;
        _liveAircraftTrackingService = new OpenSkyLiveTrackingService();
        Map = BuildMap();
        Map.Layers.Add(_liveAircraftLayer);

        _liveRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _liveRefreshTimer.Tick += async (_, _) => await RefreshLiveAircraftAsync();

        _originAirports = _flights
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
            .Concat(_flights.Select(flight => flight.Status).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value))
            .ToList();

        _filters.PropertyChanged += OnFiltersChanged;
        ApplyAirportFilter();

        _liveTrackingEnabled = liveTrackingEnabled;
        if (_liveTrackingEnabled)
        {
            StartLiveTracking();
        }
    }

    public FlightSearchFilters Filters => _filters;
    public Map Map { get; }
    public ObservableCollection<AirportOption> FilteredOriginAirports { get; } = new();
    public ObservableCollection<RouteDestinationSummary> DestinationRoutes { get; } = new();
    public ObservableCollection<LiveAircraftPosition> LiveAircraftPositions { get; } = new();
    public IReadOnlyList<string> AircraftFilters { get; }
    public IReadOnlyList<string> StatusFilters { get; }

    public string LiveTrackingButtonText => LiveTrackingEnabled ? "Stop Live Tracking" : "Start Live Tracking";

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedOriginAirport = null;
        SelectionHint = "Selection cleared. Choose an origin airport to view routes.";
        DestinationRoutes.Clear();
        _routesLayer.Features = Array.Empty<IFeature>();
        Map.Refresh();
    }

    [RelayCommand]
    private void ToggleLiveTracking()
    {
        LiveTrackingEnabled = !LiveTrackingEnabled;
    }

    partial void OnSelectedOriginAirportChanged(AirportOption? value)
    {
        RefreshSelection();
    }

    partial void OnLiveTrackingEnabledChanged(bool value)
    {
        OnPropertyChanged(nameof(LiveTrackingButtonText));

        if (value)
        {
            StartLiveTracking();
        }
        else
        {
            StopLiveTracking();
        }

        _persistPreferences();
    }

    private void OnFiltersChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FlightSearchFilters.AirportSearchText)
            or nameof(FlightSearchFilters.DepartureDate)
            or nameof(FlightSearchFilters.AircraftFilter)
            or nameof(FlightSearchFilters.StatusFilter))
        {
            ApplyAirportFilter();
            RefreshSelection();
        }
    }

    private static Map BuildMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        return map;
    }

    private void ApplyAirportFilter()
    {
        var search = Filters.AirportSearchText.Trim();
        var filtered = string.IsNullOrWhiteSpace(search)
            ? _originAirports
            : _originAirports.Where(airport =>
                airport.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                airport.Code.Contains(search, StringComparison.OrdinalIgnoreCase))
                .Where(airport => _flights.Any(flight => flight.From == airport.Code && Filters.MatchesFlight(flight)));

        FilteredOriginAirports.Clear();
        foreach (var airport in filtered)
        {
            FilteredOriginAirports.Add(airport);
        }
    }

    private void RefreshSelection()
    {
        if (SelectedOriginAirport is null)
        {
            return;
        }

        var routeSummaries = _flights
            .Where(flight => flight.From == SelectedOriginAirport.Code)
            .Where(Filters.MatchesFlight)
            .GroupBy(flight => new
            {
                flight.To,
                flight.ToName,
                flight.ToCountry,
                flight.ToLatitude,
                flight.ToLongitude
            })
            .Select(group => new RouteDestinationSummary
            {
                Route = $"{SelectedOriginAirport.Code} -> {group.Key.To}",
                DestinationName = group.Key.ToName,
                DestinationCountry = group.Key.ToCountry,
                DestinationLatitude = group.Key.ToLatitude,
                DestinationLongitude = group.Key.ToLongitude,
                FlightsCount = group.Count()
            })
            .OrderByDescending(summary => summary.FlightsCount)
            .ThenBy(summary => summary.Route)
            .ToList();

        DestinationRoutes.Clear();
        foreach (var routeSummary in routeSummaries)
        {
            DestinationRoutes.Add(routeSummary);
        }

        SelectionHint = DestinationRoutes.Count == 0
            ? $"No destination routes currently match the active filters for {SelectedOriginAirport.DisplayName}."
            : $"Showing {DestinationRoutes.Count} destination route(s) departing from {SelectedOriginAirport.DisplayName}.";

        UpdateMapRoutes(SelectedOriginAirport, DestinationRoutes);
    }

    private void UpdateMapRoutes(AirportOption origin, IEnumerable<RouteDestinationSummary> routes)
    {
        var routeFeatures = new List<IFeature>
        {
            CreateAirportPoint(origin.Latitude, origin.Longitude, Color.DarkRed)
        };

        foreach (var route in routes)
        {
            routeFeatures.Add(CreateAirportPoint(route.DestinationLatitude, route.DestinationLongitude, Color.DarkBlue));
            routeFeatures.Add(CreateRouteLine(
                origin.Latitude,
                origin.Longitude,
                route.DestinationLatitude,
                route.DestinationLongitude));
        }

        if (!Map.Layers.Any(layer => ReferenceEquals(layer, _routesLayer)))
        {
            Map.Layers.Add(_routesLayer);
        }

        _routesLayer.Features = routeFeatures;
        Map.Refresh();
    }

    private void StartLiveTracking()
    {
        _liveRefreshTimer.Start();
        _ = RefreshLiveAircraftAsync();
    }

    private void StopLiveTracking()
    {
        _liveRefreshTimer.Stop();
        LiveAircraftPositions.Clear();
        _liveAircraftLayer.Features = Array.Empty<IFeature>();
        LiveTrackingStatus = "Live tracking is off.";
        Map.Refresh();
    }

    private async Task RefreshLiveAircraftAsync()
    {
        if (!LiveTrackingEnabled || _isRefreshingLiveAircraft)
        {
            return;
        }

        _isRefreshingLiveAircraft = true;
        try
        {
            var positions = await _liveAircraftTrackingService.GetLiveAircraftPositionsAsync();
            LiveAircraftPositions.Clear();

            var features = new List<IFeature>();
            foreach (var position in positions)
            {
                LiveAircraftPositions.Add(position);
                features.Add(CreateAircraftPositionPoint(position.Latitude, position.Longitude));
            }

            _liveAircraftLayer.Features = features;
            LiveTrackingStatus = positions.Count == 0
                ? "Live tracking is on, but the API returned no aircraft positions."
                : $"Live tracking is on. Showing {positions.Count} aircraft positions from OpenSky.";
            Map.Refresh();
        }
        catch (Exception ex)
        {
            LiveTrackingStatus = $"Live tracking update failed: {ex.Message}";
        }
        finally
        {
            _isRefreshingLiveAircraft = false;
        }
    }

    private static GeometryFeature CreateAirportPoint(double latitude, double longitude, Color color)
    {
        var (x, y) = SphericalMercator.FromLonLat(longitude, latitude);
        var pointFeature = new GeometryFeature(new Point(x, y));

        pointFeature.Styles.Add(new SymbolStyle
        {
            SymbolType = SymbolType.Ellipse,
            Fill = new Brush(color),
            Outline = new Pen(Color.White, 1),
            SymbolScale = 0.8
        });

        return pointFeature;
    }

    private static GeometryFeature CreateAircraftPositionPoint(double latitude, double longitude)
    {
        var (x, y) = SphericalMercator.FromLonLat(longitude, latitude);
        var pointFeature = new GeometryFeature(new Point(x, y));

        pointFeature.Styles.Add(new SymbolStyle
        {
            SymbolType = SymbolType.Ellipse,
            Fill = new Brush(Color.LimeGreen),
            Outline = new Pen(Color.White, 1),
            SymbolScale = 0.55
        });

        return pointFeature;
    }

    private static GeometryFeature CreateRouteLine(
        double fromLatitude,
        double fromLongitude,
        double toLatitude,
        double toLongitude)
    {
        var (fromX, fromY) = SphericalMercator.FromLonLat(fromLongitude, fromLatitude);
        var (toX, toY) = SphericalMercator.FromLonLat(toLongitude, toLatitude);

        var lineString = new LineString(new[]
        {
            new Coordinate(fromX, fromY),
            new Coordinate(toX, toY)
        });

        var lineFeature = new GeometryFeature(lineString);
        lineFeature.Styles.Add(new VectorStyle
        {
            Line = new Pen(Color.DeepSkyBlue, 2.5)
        });

        return lineFeature;
    }
}
