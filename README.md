# Flight Tracker System (Avalonia + MVVM)

Project structure diagram

```text
FlightTracker/
├─ Assets/
│  └─ flights.json
├─ Models/
│  ├─ Flight.cs
│  ├─ AirportOption.cs
│  ├─ FlightSearchFilters.cs
│  ├─ RouteDestinationSummary.cs
│  └─ AnalyticsModels.cs
├─ Services/
│  ├─ FlightDataService.cs
│  ├─ FlightAnalyticsService.cs
│  ├─ UserPreferencesService.cs
│  └─ ExportService.cs
├─ ViewModels/
│  ├─ ViewModelBase.cs
│  ├─ MainWindowViewModel.cs
│  ├─ RouteVisualizationViewModel.cs
│  ├─ AirportFlightInfoViewModel.cs
│  └─ AnalyticsViewModel.cs
├─ Views/
│  ├─ MainWindow.axaml
│  ├─ RouteVisualizationView.axaml
│  ├─ AirportFlightInfoView.axaml
│  └─ AnalyticsView.axaml
├─ FlightTracker.Tests/
│  ├─ Services/
│  ├─ ViewModels/
│  └─ TestData/
└─ FlightTracker.csproj
```

## Setup instructions

1. Must have .NET 8 SDK.
2. Dependencies:
   - `dotnet restore`
3. Run the app:
   - `dotnet run --project FlightTracker.csproj`
4. Run tests:
   - `dotnet test`
