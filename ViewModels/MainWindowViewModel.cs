using FlightTracker.Models;
using FlightTracker.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FlightTracker.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly IUserPreferencesService _preferencesService;
    private readonly UserPreferences _preferences;
    private readonly List<string> _navigationHistory = [];
    private readonly Stack<int> _backStack = new();
    private readonly Stack<int> _forwardStack = new();
    private bool _isRestoringSelection;
    private int _selectedTabIndex;

    public MainWindowViewModel(
        IReadOnlyList<Flight> flights,
        IFlightAnalyticsService analyticsService,
        IExportService exportService,
        IUserPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
        _preferences = preferencesService.Load();

        Filters = new FlightSearchFilters();
        Filters.Load(_preferences);
        Filters.PropertyChanged += OnFiltersChanged;

        RouteVisualization = new RouteVisualizationViewModel(flights, Filters, _preferences.LiveTrackingEnabled, PersistPreferences);
        AirportFlightInfo = new AirportFlightInfoViewModel(flights, Filters, exportService);
        Analytics = new AnalyticsViewModel(
            flights,
            Filters,
            analyticsService,
            exportService,
            _preferences.ShowStatusChart,
            _preferences.ShowAircraftChart,
            _preferences.ShowHourlyChart,
            PersistPreferences);

        _selectedTabIndex = Math.Clamp(_preferences.SelectedTabIndex, 0, 2);
        _navigationHistory.Add(ViewNameForIndex(_selectedTabIndex));

        Analytics.PropertyChanged += OnAnalyticsChanged;
    }

    public FlightSearchFilters Filters { get; }
    public RouteVisualizationViewModel RouteVisualization { get; }
    public AirportFlightInfoViewModel AirportFlightInfo { get; }
    public AnalyticsViewModel Analytics { get; }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (value == _selectedTabIndex)
            {
                return;
            }

            var previousTabIndex = _selectedTabIndex;
            _selectedTabIndex = value;
            OnPropertyChanged(nameof(SelectedTabIndex));

            if (!_isRestoringSelection)
            {
                _backStack.Push(previousTabIndex);
                _forwardStack.Clear();
            }

            UpdateNavigationHistory(value);
            PersistPreferences();
            NavigateBackCommand.NotifyCanExecuteChanged();
            NavigateForwardCommand.NotifyCanExecuteChanged();
        }
    }

    public string NavigationHistoryText => _navigationHistory.Count == 0
        ? "Navigation history appears here as you move between views."
        : $"Navigation history: {string.Join(" -> ", _navigationHistory)}";

    public bool CanNavigateBack => _backStack.Count > 0;
    public bool CanNavigateForward => _forwardStack.Count > 0;

    [CommunityToolkit.Mvvm.Input.RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void NavigateBack()
    {
        if (_backStack.Count == 0)
        {
            return;
        }

        var targetIndex = _backStack.Pop();
        _forwardStack.Push(_selectedTabIndex);
        SetSelectedTabIndexWithoutRecording(targetIndex);
        NavigateBackCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand(CanExecute = nameof(CanNavigateForward))]
    private void NavigateForward()
    {
        if (_forwardStack.Count == 0)
        {
            return;
        }

        var targetIndex = _forwardStack.Pop();
        _backStack.Push(_selectedTabIndex);
        SetSelectedTabIndexWithoutRecording(targetIndex);
        NavigateBackCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();
    }

    private void SetSelectedTabIndexWithoutRecording(int index)
    {
        _isRestoringSelection = true;
        SelectedTabIndex = index;
        _isRestoringSelection = false;
    }

    private void OnFiltersChanged(object? sender, PropertyChangedEventArgs e)
    {
        PersistPreferences();
    }

    private void OnAnalyticsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AnalyticsViewModel.ShowStatusChart)
            or nameof(AnalyticsViewModel.ShowAircraftChart)
            or nameof(AnalyticsViewModel.ShowHourlyChart))
        {
            PersistPreferences();
        }
    }

    private void UpdateNavigationHistory(int index)
    {
        var viewName = ViewNameForIndex(index);
        if (_navigationHistory.Count == 0 || _navigationHistory[^1] != viewName)
        {
            _navigationHistory.Add(viewName);
            if (_navigationHistory.Count > 8)
            {
                _navigationHistory.RemoveAt(0);
            }

            OnPropertyChanged(nameof(NavigationHistoryText));
        }
    }

    private void PersistPreferences()
    {
        Filters.Save(_preferences);
        _preferences.SelectedTabIndex = _selectedTabIndex;
        _preferences.LiveTrackingEnabled = RouteVisualization.LiveTrackingEnabled;
        _preferences.ShowStatusChart = Analytics.ShowStatusChart;
        _preferences.ShowAircraftChart = Analytics.ShowAircraftChart;
        _preferences.ShowHourlyChart = Analytics.ShowHourlyChart;
        _preferencesService.Save(_preferences);
    }

    private static string ViewNameForIndex(int index)
    {
        return index switch
        {
            0 => "View 1: Route Visualization",
            1 => "View 2: Airport Flight Info",
            2 => "View 3: Analytics",
            _ => "Unknown View"
        };
    }
}
