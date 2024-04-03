using System.Collections.Generic;
using System.Linq;
using Csg;
using System.Numerics;

namespace Utility.Mathematics.Extensions;

public static class Vector3Extensions
{
    public static Vector3D ToVector3D(this Vector3 vector3) => new(vector3.X, vector3.Y, vector3.Z);
    public static (Vector3 Min, Vector3 Max) GetMinMax(this IList<Vector3> vertices)
    {
        var minX = vertices.Min(v => v.X);
        var maxX = vertices.Max(v => v.X);
        var minY = vertices.Min(v => v.Y);
        var maxY = vertices.Max(v => v.Y);
        var minZ = vertices.Min(v => v.Z);
        var maxZ = vertices.Max(v => v.Z);
        var topLeft = new Vector3(minX, minY, maxZ);
        var bottomRight = new Vector3(maxX, maxY, minZ);
        return (topLeft, bottomRight);
    }
}