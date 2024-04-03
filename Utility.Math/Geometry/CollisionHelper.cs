using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Utility.Mathematics.Shapes;

namespace Utility.Mathematics.Geometry;

public static class CollisionHelper
{
    public static bool Collides(IShape a, IShape b)
    {
        var x = a.GetSolid();
        var y = b.GetSolid();
        if (x.Polygons.Count == 0 || y.Polygons.Count == 0) { return false; }
        return x.Intersect(y).Polygons.Any();
    }

    public static List<Vector3> GetCollisionPoints(IShape a, IShape b)
    {
        var x = a.GetSolid();

        var y = b.GetSolid();
        if (x.Polygons.Count == 0 || y.Polygons.Count == 0) { return []; }
        var ps = x.Intersect(y).Polygons;
        return ps.SelectMany(p => p.Vertices).Select(v => new Vector3((float)v.Pos.X, (float)v.Pos.Y, (float)v.Pos.Z)).ToList();
    }
}