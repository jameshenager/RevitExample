using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace Wpf.FrameworkTest;
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel mainWindowViewModel)
    {
        DataContext = mainWindowViewModel;
        InitializeComponent();
    }
}


public partial class MainWindowViewModel() : ObservableObject
{
    public ObservableCollection<string> SelectedElementIds { get; set; } = [];

    [RelayCommand]
    public void GetSelectedElementIds()
    {

    }
}