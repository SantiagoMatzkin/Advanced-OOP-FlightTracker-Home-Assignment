using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FlightTracker.Services;
using FlightTracker.ViewModels;
using FlightTracker.Views;

namespace FlightTracker;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dataService = new JsonFlightDataService();
            var analyticsService = new FlightAnalyticsService();
            var exportService = new ExportService();
            var preferencesService = new JsonUserPreferencesService();

            var flights = dataService.LoadFlights();
            var mainWindowViewModel = new MainWindowViewModel(
                flights,
                analyticsService,
                exportService,
                preferencesService);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
