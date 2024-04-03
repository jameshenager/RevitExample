using System;
using System.Collections.Generic;
using System.Numerics;
using Utility.Mathematics.Shapes;

namespace Plugin.Core.Interfaces;

/* MoveElementWrapper */
public interface IMoveElementWrapper
{
    Vector3 Translation { set; } //ToDo: Change this to doubles
    List<long> ElementIds { set; }
}

/* ElementSelector */
public interface IElementSelector
{
    void SetSelectedElements(List<long> elementIds);
    Action<List<long>> SelectionUpdated { get; set; }
}

/* QuerySelectedElements */
public interface IQuerySelectedElements { List<long> GetSelectedElements(); }

public interface IMechanicalGeometryService
{
    IEnumerable<PipeModel> GetSelectedPipes();
    List<ConnectorModel> GetConnectors();
    List<WallModel> GetSelectedWallGeometries();
}