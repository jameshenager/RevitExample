using Autodesk.Revit.UI;
using Plugin.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace RevitPlugin;

// ReSharper disable once UnusedMember.Global
public class Startup : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        var directoryPath = @"E:\Programming\RevitPlugin\RevitPlugin\RevitPlugin\bin\x64\Debug"; //This needs to be configurable. Then we can deploy to different environments
        var prefix = "RevitPlugin.Wpf";

        var matchingAssemblies = AssemblyLoader.LoadAssembliesWithPrefix(directoryPath, prefix);
        AssemblyLoader.LoadReferencedDlls();

        foreach (var assembly in matchingAssemblies)
        {

            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            var commandTypes =
                assembly
                    .GetTypes()
                    .Where(t => t.GetCustomAttribute<RevitCommandAttribute>() != null)
                    .OrderBy(t => t.GetCustomAttribute<RevitCommandAttribute>().Order)
                    ;

            foreach (var type in commandTypes)
            {
                var attribute = type.GetCustomAttribute<RevitCommandAttribute>();

                try { application.CreateRibbonTab(attribute.Ribbon); }
                catch (Autodesk.Revit.Exceptions.ArgumentException) { }

                var panel = application.GetRibbonPanels(attribute.Ribbon).FirstOrDefault(p => p.Name == attribute.Panel) ?? application.CreateRibbonPanel(attribute.Ribbon, attribute.Panel);

                var buttonData = new PushButtonData(type.Name, attribute.ButtonText, assemblyPath, $"RevitPlugin.Screens.{attribute.CommandName}");
                if (panel.AddItem(buttonData) is PushButton button) { button.LargeImage = new BitmapImage(new Uri(attribute.ImageUri)); }
            }
        }
        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
}

public class AssemblyLoader
{
    public static IEnumerable<Assembly> LoadAssembliesWithPrefix(string directoryPath, string prefix)
    {
        var loadedAssemblies = new List<Assembly>();

        if (!Directory.Exists(directoryPath)) { throw new DirectoryNotFoundException($"The specified directory was not found: {directoryPath}"); }

        var assemblyFiles = Directory.GetFiles(directoryPath, "*.exe");

        foreach (var file in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(file);
            if (assembly.FullName.Contains(prefix)) { loadedAssemblies.Add(assembly); }
        }

        return loadedAssemblies;
    }

    public static void LoadReferencedDlls()
    {
        //I need to create another project which deploys the dlls to the correct location and sets the `.addin` file.
        //Probably have different ones for different environments.
        var assemblyFile = @"E:\Programming\RevitPlugin\RevitPlugin\RevitPlugin.Wpf\bin\x64\Debug\DotNetCsg.dll";
        if (!File.Exists(assemblyFile)) { return; }
        Assembly.LoadFrom(assemblyFile);
    }
}