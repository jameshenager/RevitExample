using Plugin.Core;
using Plugin.Core.Mocks;
using System.Windows;
using Utility.Mathematics.Geometry;

namespace RevitPlugin.Wpf.PipeTool;
public partial class App
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        if (AssemblyHelper.IsRunningInRevit()) { return; /* Revit-specific initialization: Since this is handled by Revit plugin startup, there's nothing to do anything here */ }
        var vm = new PipeToolMainWindowViewModel(new MockMoveElementWrapper(), new MockableEvent(), new MockSelectedElements(), new MockElementSelector(), new MockMechanicalMechanicalGeometryService(), new CollisionManager());
        var mainWindow = new PipeToolMainWindowView(vm);
        mainWindow.Show();
    }
}