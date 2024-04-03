using System.Windows;

namespace Wpf.CoreTest;
public partial class MainWindow2 : Window
{
    public MainWindow2(MainWindowViewModel2 mainWindowViewModel2)
    {
        DataContext = mainWindowViewModel2;
        InitializeComponent();
    }
}