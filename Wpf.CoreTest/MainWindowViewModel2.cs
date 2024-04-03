using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitPlugin.Core;

namespace Wpf.CoreTest;

public partial class MainWindowViewModel2(ExternalCommandData commandData, ElementMoverEventHandler elementMoverEventHandler, ExternalEvent exEvent) : ObservableObject
{
    [ObservableProperty] private string _selectedItemString = string.Empty; //This breaks the code for some reason.

    //private string _selectedItemString;
    //public string SelectedItemString { get => _selectedItemString; set => SetProperty(ref _selectedItemString, value); }

    [RelayCommand]
    public void GetSelectedElementIds()
    {
        var uidoc = commandData.Application.ActiveUIDocument;
        var selectedIds = uidoc.Selection.GetElementIds();

        SelectedItemString = string.Join(Environment.NewLine, selectedIds.Select(id => id.Value.ToString()));
    }

    [RelayCommand]
    public void MoveSelectedElements()
    {
        try
        {
            var uiapp = commandData.Application;
            var doc = uiapp.ActiveUIDocument.Document;
            var translation = new XYZ(0, 0, 10);

            var selectedElementIds =
                SelectedItemString
                    .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Select(long.Parse)
                    .ToList();

            elementMoverEventHandler.SelectedElementIds = selectedElementIds;
            elementMoverEventHandler.Translation = translation;
            elementMoverEventHandler.Doc = doc;

            exEvent.Raise();
        }
        catch (Exception e) { MessageBox.Show(e.Message); }
    }
}