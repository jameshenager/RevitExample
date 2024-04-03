using System.Collections.Generic;
using System.Numerics;
using Utility.Mathematics.Shapes;

namespace Plugin.Core;

//ToDo: I should set up a series of obstructions in a 3d space, then use a stacked series of pipes to figure out which pipes are obstructed and what the optimal path is. This would look good for a demo.

public class ConnectorModelInformation
{
    //Figure out which other connectorModels it intersects with
    public List<long> IntersectConnectorId { get; set; } = [];
    public List<CylinderModel> Cylinders { get; set; }
}

public class ConnectorModel
{
    public long ElementId { get; set; }
    public Vector3 Position { get; set; }
    public double Diameter { get; set; }
    public List<ConnectionPoint> ConnectionPoints { get; set; } = [];
}

public class ConnectionPoint
{
    public required long ElementId { get; set; }
    public Vector3 Direction { get; set; }
    public Vector3 Position { get; set; }
    public double Diameter { get; set; }
    public bool IsOutput { get; set; }
    public CylinderModel CylinderModel => new(new(Position - Direction, Position + Direction) /* a bit of a hack since connections can face away from one another */, (float)Diameter * 2, ElementId);
}

//ToDo: Count how many rectangles I'm drawing and try to combine them
//ToDo: Create unit tests for the collision detection