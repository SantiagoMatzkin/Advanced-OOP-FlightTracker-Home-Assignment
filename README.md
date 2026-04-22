# Flight Tracker System (Avalonia + MVVM)

This project implements a multi-view Flight Tracker System in .NET/Avalonia following the MVVM pattern.  
Flight data is loaded from `Assets/flights.json`, rendered across three functional views, and can be exported as CSV/JSON.

## Implemented features

1. **View 1 - Route Visualization (Mapsui)**
   - Search and select origin airports.
   - Filter routes by departure date, aircraft type, and status.
   - Display destination route cards with traffic counts.
   - Draw airport markers and route lines on a Mapsui map.
   - Clear current selection and reset map state.
2. **View 2 - Airport Flight Info**
   - Select airport and filter by departure date, aircraft type, and status.
   - Show flight number, destination, departure UTC, aircraft, airline, and status.
   - Export filtered flight rows to CSV.
3. **View 3 - Analytics with LINQ (LiveCharts2)**
   - Top airlines by traffic volume.
   - Busiest route per time-of-day bucket.
   - Country-level traffic trends.
   - Dynamic optional charts for status, aircraft usage, and departure-hour patterns.
   - Export analytics output to JSON.
4. **Bonus features**
   - Persistent user preferences stored locally in `%LocalAppData%\FlightTracker\user-preferences.json`.
   - Navigation back/forward controls and a visible navigation log in the shell header.
   - Shared filter state across all three views so search criteria stay synchronized.
5. **Unit tests (xUnit)**
   - ViewModel filtering logic.
   - LINQ analytics query results.

## UI mockup/prototype

The implemented layout follows this wireframe:

```text
┌─────────────────────────────────────────────────────────────────────────┐
│ Header: Flight Tracker System                                          │
│         [Back] [Forward]  History: View 1 -> View 2 -> View 3         │
├─────────────────────────────────────────────────────────────────────────┤
│ Tabs: [View 1 Routes] [View 2 Airport Info] [View 3 Analytics]        │
├─────────────────────────────────────────────────────────────────────────┤
│ View 1: [Search + Date/Aircraft/Status filters + Airport list] | map   │
│ View 2: [Airport + Date/Aircraft/Status filters + Export] + flights    │
│ View 3: [Filters + Export + chart toggles] + base charts + extras      │
└─────────────────────────────────────────────────────────────────────────┘
```

## Project structure diagram

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

1. Install .NET 8 SDK.
2. Restore dependencies:
   - `dotnet restore`
3. Run the application:
   - `dotnet run --project FlightTracker.csproj`
4. Run tests:
   - `dotnet test`

## Bonus features

- **Persistent preferences:** filter selections, tab position, and optional charts are stored locally and restored on the next launch.
- **Advanced search:** the shared filter strip updates route visualization, airport flight info, and analytics from the same criteria.
- **Dynamic charts:** analytics includes three optional charts that can be shown or hidden individually.
- **Navigation history:** the shell exposes back/forward commands and a live history trail.

## Component notes

- **Data source:** `Assets/flights.json` is copied to output and loaded by `JsonFlightDataService`.
- **Map rendering:** `RouteVisualizationViewModel` builds route overlays using Mapsui + `MemoryLayer`.
- **Analytics:** `FlightAnalyticsService` performs all chart aggregations via LINQ.
- **Shared filters:** `FlightSearchFilters` propagates the active date, aircraft, status, and airport search criteria across all views.
- **Preferences:** `JsonUserPreferencesService` stores UI state under the user's local application data folder.
- **Exports:**
  - Flights -> `Exports/flights-*.csv`
  - Analytics -> `Exports/analytics-*.json`
