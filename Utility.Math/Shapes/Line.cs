using System.Numerics;

namespace Utility.Mathematics.Shapes;

public class Line(Vector3 start, Vector3 end)
{
    public Vector3 Start { get; set; } = start;
    public Vector3 End { get; set; } = end;
}