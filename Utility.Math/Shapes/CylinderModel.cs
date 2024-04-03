using Csg;
using System;
using System.Collections.Generic;
using System.Numerics;
using Utility.Mathematics.Extensions;

namespace Utility.Mathematics.Shapes;

public class CylinderModel(Line axis, float radius, long elementId) : IShape
{
    public List<Vector3> CollisionPoints { get; set; } = [];
    public List<long> CollisionElementIds { get; set; } = [];
    public long ElementId { get; set; } = elementId;
    public Line Axis { get; set; } = axis;
    public float Radius { get; set; } = radius;

    public Solid GetSolid() => Solid.Value;
    public Lazy<Solid> Solid => new(() =>
    {
        var cylinderOptions = new CylinderOptions()
        {
            Start = Axis.Start.ToVector3D(),
            End = Axis.End.ToVector3D(),
            RadiusStart = Radius,
            RadiusEnd = Radius,
        };
        return Solids.Cylinder(cylinderOptions);
    });
    public List<Vector3> GetBoundaryPoints()
    {
        var results = new HashSet<Vector3>();
        var direction = Axis.End - Axis.Start;
        var length = direction.Length();
        direction = Vector3.Normalize(direction);

        var step = Math.Min(1.0, Radius * 2);
        double radius = Radius;

        for (double distance = 0; distance <= length; distance += step)
        {
            var currentPoint = Axis.Start + direction * (float)distance;
            ShapeHelper.AddPointsAround(currentPoint, results, radius);
        }

        return [.. results,];
    }
}