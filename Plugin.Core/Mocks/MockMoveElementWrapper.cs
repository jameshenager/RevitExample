using Plugin.Core.Interfaces;
using System.Collections.Generic;
using System.Numerics;

namespace Plugin.Core.Mocks;

public class MockMoveElementWrapper : IMoveElementWrapper
{
    public Vector3 Translation { get; set; }
    public List<long> ElementIds { get; set; }
}