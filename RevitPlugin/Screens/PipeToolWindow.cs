using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Plugin.Core;
using Revit.Core;
using RevitPlugin.Wpf.PipeTool;
using Utility.Mathematics.Geometry;

namespace RevitPlugin.Screens;

// ReSharper disable once UnusedMember.Global
[Transaction(TransactionMode.Manual)]
public class PipeToolWindow : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        /* Move */
        var moveElementsCommand = new MoveElementsCommand();
        var movementExternalEventHandler = new ExternalEventHandler(commandData.Application, moveElementsCommand);
        var exMoveEvent = ExternalEvent.Create(movementExternalEventHandler);
        var elementMoverEvent = new MockableEvent() { RaiseAction = () => exMoveEvent.Raise(), };

        /* Select */
        var selectElementsCommand = new SelectElementsCommand(commandData.Application);
        var selectExternalEventHandler = new ExternalEventHandler(commandData.Application, selectElementsCommand);
        var exSelectEvent = ExternalEvent.Create(selectExternalEventHandler);
        var elementSelectorEvent = new MockableEvent() { RaiseAction = () => exSelectEvent.Raise(), };
        var elementSelector = new RevitElementSelector(selectElementsCommand, elementSelectorEvent);

        /* Query */
        var selectedElementQueryService = new RevitQuerySelectedElementsQueryService(commandData.Application);

        var revitPipeGeometryService = new RevitGeometryService(commandData.Application);

        var collisionManager = new CollisionManager();

        var vm = new PipeToolMainWindowViewModel(new RevitMoveElementWrapper(movementExternalEventHandler, moveElementsCommand), elementMoverEvent, selectedElementQueryService, elementSelector, revitPipeGeometryService, collisionManager);
        var v = new PipeToolMainWindowView(vm);
        v.Show();

        return Result.Succeeded;
    }
}