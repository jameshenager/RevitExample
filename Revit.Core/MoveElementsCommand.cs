using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Plugin.Core;
using Plugin.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

// ReSharper disable UseCollectionExpression
// ReSharper disable RedundantNameQualifier
namespace Revit.Core;

/* General */
public class ExternalEventHandler(UIApplication uIApplication, IRevitCommand revitCommand) : IExternalEventHandler
{
    public void Execute(UIApplication app) => revitCommand.Execute(uIApplication.ActiveUIDocument.Document);
    public string GetName() => revitCommand.GetName();
}

/* Update Only */
public class MoveElementsCommand : IRevitCommand
{
    public List<long> ElementIds { get; set; }
    public System.Numerics.Vector3 Translation { get; set; }
    public string GetName() => nameof(MoveElementsCommand);

    public void Execute(Document doc)
    {
        using var transaction = new Transaction(doc, nameof(MoveElementsCommand));
        transaction.Start();

        try
        {
            var elementIds = ElementIds.Select(eid => new ElementId(eid)).ToList();
            ElementTransformUtils.MoveElements(doc, elementIds, new(Translation.X, Translation.Y, Translation.Z));
            transaction.Commit();
        }
        catch (Exception e) //this happens if I try to move an item which has been deleted. Might also happen when trying to move a pinned element.
        {
            transaction.RollBack();
            throw new Exception($"Error moving elements: {e.Message}"); // ToDo: I need a way to show this to the user.
        }
    }
}

public class RevitMoveElementWrapper : IMoveElementWrapper
{
    public RevitMoveElementWrapper
        (
            IExternalEventHandler minimumMovementExternalEventHandler,
            MoveElementsCommand moveElementsCommand
        )
    { _moveElementsCommand = moveElementsCommand; _ = minimumMovementExternalEventHandler; /* Don't let it die. */    }

    public System.Numerics.Vector3 Translation { set => _moveElementsCommand.Translation = value; }
    public List<long> ElementIds { set => _moveElementsCommand.ElementIds = value; }
    private readonly MoveElementsCommand _moveElementsCommand;
}

/* Query Only */
public class RevitQuerySelectedElementsQueryService(UIApplication uiApplication) : IQuerySelectedElements
{
    public List<long> GetSelectedElements() => uiApplication.ActiveUIDocument.Selection.GetElementIds().Select(id => id.Value).ToList();
}

/* Update and Query */
public class SelectElementsCommand(UIApplication uiApplication) : IRevitCommand
{
    public List<long> DesiredElementIds { get; set; }
    public string GetName() => nameof(SelectElementsCommand);
    public Action<List<long>> OnCompletion { get; set; }
    public void Execute(Document doc) // Called from _elementSelectorEvent.Raise() whenever Revit wants.
    {
        using var transaction = new Transaction(doc, "Move Elements");

        transaction.Start();
        uiApplication.ActiveUIDocument.Selection.SetElementIds(new List<ElementId> { Capacity = 0, });

        uiApplication.ActiveUIDocument.Selection.SetElementIds(DesiredElementIds.Select(id => new ElementId(id)).ToList());
        transaction.Commit();

        OnCompletion?.Invoke(uiApplication.ActiveUIDocument.Selection.GetElementIds().Select(id => id.Value).ToList()); // Some of the IDs we requested might not be available because they were deleted, or maybe they can never be moved.
    }
}

public class RevitElementSelector : IElementSelector
{
    private readonly SelectElementsCommand _selectElementsCommand;
    private readonly MockableEvent _elementSelectorEvent;

    public RevitElementSelector(SelectElementsCommand selectElementsCommand, MockableEvent elementSelectorEvent)
    {
        _selectElementsCommand = selectElementsCommand;
        _elementSelectorEvent = elementSelectorEvent;
        _selectElementsCommand.OnCompletion = selectedElementIds => { SelectionUpdated?.Invoke(selectedElementIds); };
    }

    public Action<List<long>> SelectionUpdated { get; set; }
    public void SetSelectedElements(List<long> elementIds)
    {
        _selectElementsCommand.DesiredElementIds = elementIds; // Set data
        _elementSelectorEvent.Raise();// Wait for Revit to take its sweet time.
    }
}

public static class LocationPointExtensions
{
    public static Vector3 ToVector3(this LocationPoint locationPoint)
    {
        var point = locationPoint.Point;
        return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
    }

    public static Vector3 ToVector3(this Location location) => location is LocationPoint locationPoint ? locationPoint.ToVector3() : new Vector3(0, 0, 0);
    public static Vector3 ToVector3(this XYZ xyz) => new((float)xyz.X, (float)xyz.Y, (float)xyz.Z);
}