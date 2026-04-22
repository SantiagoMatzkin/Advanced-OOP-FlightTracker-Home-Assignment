using Avalonia.Controls;
using FlightTracker.ViewModels;

namespace FlightTracker.Views;

public partial class RouteVisualizationView : UserControl
{
    public RouteVisualizationView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is RouteVisualizationViewModel viewModel)
        {
            RouteMapControl.Map = viewModel.Map;
        }
    }
}
