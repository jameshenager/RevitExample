using Csg;
using System;
using System.Collections.Generic;
using System.Numerics;
using Utility.Mathematics.Extensions;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Utility.Mathematics.Shapes;

public class PipeModel : IShape
{
    public List<Vector3> CollisionPoints { get; set; } = [];
    public List<long> CollisionElementIds { get; set; } = [];
    public required long ElementId { get; set; }
    public required Vector3 StartPoint { get; set; }
    public required Vector3 EndPoint { get; set; }
    public required double Diameter { get; set; }
    public Solid GetSolid() => Solid.Value;
    public Lazy<Solid> Solid => new(() =>
    {
        var cylinderOptions = new CylinderOptions()
        {
            Start = StartPoint.ToVector3D(),
            End = EndPoint.ToVector3D(),
            RadiusStart = Diameter / 2,
            RadiusEnd = Diameter / 2,
        };
        return Solids.Cylinder(cylinderOptions);
    });
    public List<Vector3> GetBoundaryPoints()
    {
        var results = new HashSet<Vector3>();
        var direction = EndPoint - StartPoint;
        var length = direction.Length();
        direction = Vector3.Normalize(direction);

        var step = Math.Min(0.8, Diameter);
        var radius = Diameter / 2;

        for (double distance = 0; distance <= length; distance += step)
        {
            var currentPoint = StartPoint + direction * (float)distance;
            ShapeHelper.AddPointsAround(currentPoint, results, radius);
        }

        return [.. results,];
    }
    public override int GetHashCode() => ElementId.GetHashCode() ^ StartPoint.GetHashCode() ^ EndPoint.GetHashCode() ^ Diameter.GetHashCode();
}