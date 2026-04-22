using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlightTracker.Models;
using FlightTracker.Services;
using System.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace FlightTracker.ViewModels;

public sealed partial class AnalyticsViewModel : ViewModelBase
{
    private readonly IReadOnlyList<Flight> _flights;
    private readonly FlightSearchFilters _filters;
    private readonly IFlightAnalyticsService _analyticsService;
    private readonly IExportService _exportService;
    private readonly Action _persistPreferences;

    private IReadOnlyList<Flight> _visibleFlights = [];

    [ObservableProperty]
    private string _analyticsExportMessage = "Run analytics directly from LINQ-based data projections.";

    [ObservableProperty]
    private bool _showStatusChart;

    [ObservableProperty]
    private bool _showAircraftChart;

    [ObservableProperty]
    private bool _showHourlyChart;

    public AnalyticsViewModel(
        IReadOnlyList<Flight> flights,
        FlightSearchFilters filters,
        IFlightAnalyticsService analyticsService,
        IExportService exportService)
        : this(flights, filters, analyticsService, exportService, false, false, false, () => { })
    {
    }

    public AnalyticsViewModel(
        IReadOnlyList<Flight> flights,
        FlightSearchFilters filters,
        IFlightAnalyticsService analyticsService,
        IExportService exportService,
        bool showStatusChart,
        bool showAircraftChart,
        bool showHourlyChart,
        Action persistPreferences)
    {
        _flights = flights;
        _filters = filters;
        _analyticsService = analyticsService;
        _exportService = exportService;
        _persistPreferences = persistPreferences;
        _showStatusChart = showStatusChart;
        _showAircraftChart = showAircraftChart;
        _showHourlyChart = showHourlyChart;

        _filters.PropertyChanged += OnFiltersChanged;
        RebuildAnalytics();
    }

    public FlightSearchFilters Filters => _filters;

    public IReadOnlyList<AirlineTrafficMetric> TopAirlineMetrics { get; private set; } = [];
    public IReadOnlyList<TimeOfDayRouteMetric> BusiestRoutesByTimeOfDay { get; private set; } = [];
    public IReadOnlyList<CountryTrafficMetric> CountryTrafficMetrics { get; private set; } = [];
    public IReadOnlyList<FlightStatusMetric> FlightStatusMetrics { get; private set; } = [];
    public IReadOnlyList<AircraftUsageMetric> AircraftUsageMetrics { get; private set; } = [];
    public IReadOnlyList<DepartureHourMetric> DepartureHourMetrics { get; private set; } = [];

    public ISeries[] AirlineSeries { get; private set; } = [];
    public Axis[] AirlineXAxes { get; private set; } = [];
    public ISeries[] TimeOfDayRouteSeries { get; private set; } = [];
    public Axis[] TimeOfDayXAxes { get; private set; } = [];
    public ISeries[] CountryTrafficSeries { get; private set; } = [];
    public ISeries[] StatusBreakdownSeries { get; private set; } = [];
    public Axis[] StatusBreakdownXAxes { get; private set; } = [];
    public ISeries[] AircraftUsageSeries { get; private set; } = [];
    public Axis[] AircraftUsageXAxes { get; private set; } = [];
    public ISeries[] DepartureHourSeries { get; private set; } = [];
    public Axis[] DepartureHourXAxes { get; private set; } = [];

    public string StatusChartButtonText => ShowStatusChart ? "Remove Status Chart" : "Add Status Chart";
    public string AircraftChartButtonText => ShowAircraftChart ? "Remove Aircraft Chart" : "Add Aircraft Chart";
    public string HourlyChartButtonText => ShowHourlyChart ? "Remove Hourly Chart" : "Add Hourly Chart";

    partial void OnShowStatusChartChanged(bool value)
    {
        OnPropertyChanged(nameof(StatusChartButtonText));
        _persistPreferences();
    }

    partial void OnShowAircraftChartChanged(bool value)
    {
        OnPropertyChanged(nameof(AircraftChartButtonText));
        _persistPreferences();
    }

    partial void OnShowHourlyChartChanged(bool value)
    {
        OnPropertyChanged(nameof(HourlyChartButtonText));
        _persistPreferences();
    }

    [RelayCommand]
    private void ExportAnalytics()
    {
        var snapshot = _analyticsService.BuildExportSnapshot(_visibleFlights);
        var exportPath = _exportService.ExportAnalyticsJson(snapshot);
        AnalyticsExportMessage = $"Analytics exported to: {exportPath}";
    }

    [RelayCommand]
    private void ToggleStatusChart() => ShowStatusChart = !ShowStatusChart;

    [RelayCommand]
    private void ToggleAircraftChart() => ShowAircraftChart = !ShowAircraftChart;

    [RelayCommand]
    private void ToggleHourlyChart() => ShowHourlyChart = !ShowHourlyChart;

    private void OnFiltersChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FlightSearchFilters.DepartureDate)
            or nameof(FlightSearchFilters.AircraftFilter)
            or nameof(FlightSearchFilters.StatusFilter))
        {
            RebuildAnalytics();
        }
    }

    private void RebuildAnalytics()
    {
        _visibleFlights = _flights.Where(_filters.MatchesFlight).ToList();

        TopAirlineMetrics = _analyticsService.GetTopAirlines(_visibleFlights, 6);
        BusiestRoutesByTimeOfDay = _analyticsService.GetBusiestRoutesByTimeOfDay(_visibleFlights);
        CountryTrafficMetrics = _analyticsService.GetCountryTraffic(_visibleFlights);
        FlightStatusMetrics = _analyticsService.GetStatusBreakdown(_visibleFlights);
        AircraftUsageMetrics = _analyticsService.GetAircraftUsage(_visibleFlights);
        DepartureHourMetrics = _analyticsService.GetDepartureHours(_visibleFlights);

        AirlineSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Flights",
                Values = TopAirlineMetrics.Select(metric => metric.FlightsCount).ToArray()
            }
        };

        AirlineXAxes = new[]
        {
            new Axis
            {
                Labels = TopAirlineMetrics.Select(metric => metric.Airline).ToArray(),
                LabelsRotation = 12
            }
        };

        TimeOfDayRouteSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Busiest route flights",
                Values = BusiestRoutesByTimeOfDay.Select(metric => metric.FlightsCount).ToArray()
            }
        };

        TimeOfDayXAxes = new[]
        {
            new Axis
            {
                Labels = BusiestRoutesByTimeOfDay.Select(metric => metric.TimeOfDay).ToArray()
            }
        };

        CountryTrafficSeries = CountryTrafficMetrics
            .Select(metric => new PieSeries<int>
            {
                Name = metric.Country,
                Values = new[] { metric.FlightsCount }
            })
            .Cast<ISeries>()
            .ToArray();

        StatusBreakdownSeries = FlightStatusMetrics
            .Select(metric => new PieSeries<int>
            {
                Name = metric.Status,
                Values = new[] { metric.FlightsCount }
            })
            .Cast<ISeries>()
            .ToArray();

        StatusBreakdownXAxes = new[]
        {
            new Axis
            {
                Labels = FlightStatusMetrics.Select(metric => metric.Status).ToArray()
            }
        };

        AircraftUsageSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Flights",
                Values = AircraftUsageMetrics.Select(metric => metric.FlightsCount).ToArray()
            }
        };

        AircraftUsageXAxes = new[]
        {
            new Axis
            {
                Labels = AircraftUsageMetrics.Select(metric => metric.AircraftType).ToArray(),
                LabelsRotation = 12
            }
        };

        DepartureHourSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Flights",
                Values = DepartureHourMetrics.Select(metric => metric.FlightsCount).ToArray()
            }
        };

        DepartureHourXAxes = new[]
        {
            new Axis
            {
                Labels = DepartureHourMetrics.Select(metric => metric.HourLabel).ToArray(),
                LabelsRotation = 12
            }
        };

        OnPropertyChanged(nameof(TopAirlineMetrics));
        OnPropertyChanged(nameof(BusiestRoutesByTimeOfDay));
        OnPropertyChanged(nameof(CountryTrafficMetrics));
        OnPropertyChanged(nameof(FlightStatusMetrics));
        OnPropertyChanged(nameof(AircraftUsageMetrics));
        OnPropertyChanged(nameof(DepartureHourMetrics));
        OnPropertyChanged(nameof(AirlineSeries));
        OnPropertyChanged(nameof(AirlineXAxes));
        OnPropertyChanged(nameof(TimeOfDayRouteSeries));
        OnPropertyChanged(nameof(TimeOfDayXAxes));
        OnPropertyChanged(nameof(CountryTrafficSeries));
        OnPropertyChanged(nameof(StatusBreakdownSeries));
        OnPropertyChanged(nameof(StatusBreakdownXAxes));
        OnPropertyChanged(nameof(AircraftUsageSeries));
        OnPropertyChanged(nameof(AircraftUsageXAxes));
        OnPropertyChanged(nameof(DepartureHourSeries));
        OnPropertyChanged(nameof(DepartureHourXAxes));
    }
}
