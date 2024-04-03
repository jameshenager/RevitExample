using System.Numerics;
using System.Windows.Media.Media3D;

namespace Wpf.Common;

public static class VectorHelper
{
    public static Point3D ToPoint3D(this Vector3 vector3) => new(vector3.X, vector3.Y, vector3.Z);
}