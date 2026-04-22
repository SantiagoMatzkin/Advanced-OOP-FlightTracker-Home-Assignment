# Home Assignment 3 - Flight Tracker System

## Description

You are asked to design and build a Flight Tracker System.

## Scenario

Air traffic is becoming denser as technology progresses and demand increases. It is estimated that Europe handles around 35,000 daily flights [[source](https://www.eurocontrol.int/sites/default/files/2025-01/eurocontrol-european-aviation-overview-20250123-2024-review.pdf)].
Managing all of this data is a demanding task.
For this reason, Sønderborg Airport has hired you to develop a Flight Tracker System that helps air traffic controllers view flight routes and extract useful data, while also helping airport administrators plan business strategies more efficiently.

## Non-Functional Requirements

- The application must be developed in .NET with the Avalonia Framework.
- The application must follow the MVVM pattern.
- The UI implementation must be based on a mockup/prototype.
- The UI must be intuitive and user-friendly.
- The UI must support three views (multi-view).
- Flight data must be retrieved from the provided file (flights.json).

## Functional Requirements

- **View 1:**
  - **Route visualization:** Present the flight connections from a selected airport to its available destination airports in a clear visual form. You may use the external library [Mapsui](https://mapsui.com/v5/) as your map solution for representing maps and routes.
  - **Route exploration:** Provide interactive controls, such as search by name, selection, or clearing the current selection, that allow the user to explore available routes. The displayed map and route information should update based on user interaction.
- **View 2:**
  - **Airport's flight info:** Allow the user to select an airport from a list and view related flight details. The displayed information may include flight number, destination airport, departure date and time, aircraft type, and flight status. The user should filter the view based on flight status.
- **View 3:**
  - **Analytics with LINQ:** System should render three analytics views using LINQ queries that visualize meaningful insights. Example analytics may include busiest routes by time of day, top airlines by traffic volume, or country-level traffic trends.
- **Data export:** System should support at least one export format for flight data and one export format for analytics output. The export format may be textual, tabular, graphical, or other.

## Unit-Testing

Implement test cases that cover at least ViewModel logic and LINQ query results.

## Allowed Libraries

- MVVM toolkit
- xUnit and Headless for Avalonia
- LiveCharts2
- [Mapsui](https://mapsui.com/v5/)
