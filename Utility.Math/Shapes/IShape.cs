using Csg;
using System.Collections.Generic;
using System.Numerics;
using System;

namespace Utility.Mathematics.Shapes;

public interface IShape
{
    long ElementId { get; set; }
    public Solid GetSolid();
    List<Vector3> GetBoundaryPoints();
    //I should also have a list of ElementIds that it can never intersect with.
    //I should also have a list of ElementIds that it can only intersect with.
    //Maybe also whether it can intersect (openings)
    public List<Vector3> CollisionPoints { get; set; }
    public List<long> CollisionElementIds { get; set; }
}


public class ShapeHelper
{
    public static void AddPointsAround(Vector3 point, HashSet<Vector3> result, double radius)
    {
        for (var x = -radius; x <= radius; x += radius)
        {
            for (var y = -radius; y <= radius; y += radius)
            {
                for (var z = -radius; z <= radius; z++)
                {
                    var checkPoint = new Vector3((float)(point.X + x), (float)(point.Y + y), (float)(point.Z + z));
                    result.Add(new Vector3((float)Math.Floor(checkPoint.X), (float)Math.Floor(checkPoint.Y), (float)Math.Floor(checkPoint.Z)) + new Vector3(0.5f, 0.5f, 0.5f));
                    result.Add(new Vector3((float)Math.Ceiling(checkPoint.X), (float)Math.Ceiling(checkPoint.Y), (float)Math.Ceiling(checkPoint.Z)) + new Vector3(0.5f, 0.5f, 0.5f));
                }
            }
        }
    }
}