using Csg;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Utility.Mathematics.Shapes;

public class RectangleModel : IShape
{
    public List<Vector3> CollisionPoints { get; set; } = [];
    public List<long> CollisionElementIds { get; set; } = [];
    public required long ElementId { get; set; }
    public Vector3 TopLeft { get; set; } /* actual orientation is arbitrary. I just need corners that are opposite to one another. */
    public Vector3 BottomRight { get; set; }
    public Vector3 Normal { get; set; } /* Figure out if Normal goes into the wall or outside the wall */
    public double Thickness { get; set; }
    public Vector3 Origin { get; set; }
    public bool IsOpening { get; set; }
    public Vector3 GetCenter() => (TopLeft + BottomRight) / 2;

    public List<List<Vector3>> GetTwoTriangles()
    {
        var top = new List<Vector3>
        {
            new(TopLeft.X, TopLeft.Y, TopLeft.Z),
            new(TopLeft.X, TopLeft.Y, BottomRight.Z),
            new(BottomRight.X, BottomRight.Y, TopLeft.Z),
        };
        var bottom = new List<Vector3>
        {
            new(TopLeft.X, TopLeft.Y, BottomRight.Z),
            new(BottomRight.X, BottomRight.Y, BottomRight.Z),
            new(BottomRight.X, BottomRight.Y, TopLeft.Z),
        };
        var result = new List<List<Vector3>>() { top, bottom, };
        return result;
    }

    public bool ContainsPoint(Vector3 point)
    {
        var minX = Math.Min(TopLeft.X, BottomRight.X);
        var maxX = Math.Max(TopLeft.X, BottomRight.X);
        var minY = Math.Min(TopLeft.Y, BottomRight.Y);
        var maxY = Math.Max(TopLeft.Y, BottomRight.Y);
        var minZ = Math.Min(TopLeft.Z, BottomRight.Z);
        var maxZ = Math.Max(TopLeft.Z, BottomRight.Z);

        var xWorks = (point.X >= minX && point.X <= maxX);
        var yWorks = point.Y >= minY && point.Y <= maxY;
        var zWorks = point.Z >= minZ && point.Z <= maxZ;
        return xWorks && yWorks && zWorks;
    }

    public Solid GetSolid()
    {
        if (IsOpening) { return Solids.Cube(0, true); }
        var thickness = Math.Max(Thickness, 0.01);
        var flatTopLeft = TopLeft with { Z = 0, };
        var flatBottomRight = BottomRight with { Z = 0, };
        var diff = flatBottomRight - flatTopLeft;
        var x = Vector3.Distance(flatTopLeft, flatBottomRight);
        var top = Math.Max(TopLeft.Z, BottomRight.Z);
        var bottom = Math.Min(TopLeft.Z, BottomRight.Z);
        var height = top - bottom;
        var box = Solids.Cube(
            new Vector3D(x, thickness, height),
            new Vector3D(0, 0, bottom + height / 2)
        );

        Normal = Vector3.Normalize(Normal);
        var angleRads = Math.Atan2(diff.Y, diff.X);
        var angleDegrees = angleRads * 180 / Math.PI;
        box = box.RotateZ(angleDegrees);
        var center = GetCenter();
        box = box.Translate(new Vector3D(center.X, center.Y, 0));
        //var somethingToLookAt = box.Polygons.Select(p => p.BoundingBox).ToList();
        return box;
    }

    public List<Vector3> GetBoundaryPoints()
    {
        if (IsOpening) { return []; }
        var result = new HashSet<Vector3>();
        var strideDistance = Math.Max(Thickness * 2, 1);

        var minZ = Math.Min(TopLeft.Z, BottomRight.Z);
        var maxZ = Math.Max(TopLeft.Z, BottomRight.Z);

        var topRight = BottomRight with { Z = maxZ, };

        var slope = TopLeft - topRight;
        var length = slope.Length();
        //var corners = new List<Vector3> { TopLeft, topRight, BottomRight, bottomLeft, };

        for (var z = minZ; z <= maxZ; z += (float)strideDistance)
        {
            //then go from TopLeft to topRight taking the z value and checking around there in a for loop
            var divisions = (int)(length / strideDistance);
            for (var i = 0; i <= divisions; i++)
            {
                var pointOnEdge = Vector3.Lerp(TopLeft, topRight, (float)i / divisions);
                pointOnEdge = pointOnEdge with { Z = z, };
                ShapeHelper.AddPointsAround(pointOnEdge, result, strideDistance);
            }
        }

        return [.. result,];
    }
}