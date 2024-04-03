using Autodesk.Revit.DB;

namespace Revit.Core;

public interface IRevitCommand
{
    void Execute(Document doc);
    string GetName();
}