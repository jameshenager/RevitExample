using System.Windows;

namespace RevitPlugin.Wpf.PipeTool;
public partial class PipeToolMainWindowView
{
    private readonly PipeToolMainWindowViewModel _mainWindowViewModel;
    public PipeToolMainWindowView(PipeToolMainWindowViewModel mainWindowViewModel)
    {
        DataContext = mainWindowViewModel;
        _mainWindowViewModel = mainWindowViewModel;
        InitializeComponent();
        _mainWindowViewModel.ZoomExtentsAction += ResetView;
    }

    // Only in Revit: 'Could not load file or assembly 'Microsoft.Xaml.Behaviors, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The system cannot find the file specified.'
    private void Window_Loaded(object sender, RoutedEventArgs e) => _mainWindowViewModel.UserControlLoaded();
    private void Window_Closed(object sender, System.EventArgs e)
    {
        _mainWindowViewModel.UserControlUnloaded();
        _mainWindowViewModel.ZoomExtentsAction -= ZoomExtents;
    }

    private void RedrawGrid() { /*ViewPort.Width = 100; //this is actual viewable width */ /* ToDo: Figure out how to redraw the gridlines */ }
    private void ZoomExtents() => ViewPort.ZoomExtents();
    private void ResetView()
    {
        ZoomExtents();
        RedrawGrid();
    }
}