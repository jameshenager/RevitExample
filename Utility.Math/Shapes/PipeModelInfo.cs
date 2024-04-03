using System.Collections.Generic;

namespace Utility.Mathematics.Shapes;

public class PipeModelInfo(PipeModel pipe)
{
    public PipeModel Pipe { get; } = pipe;
    public List<long> Intersects { get; set; } = [];
}