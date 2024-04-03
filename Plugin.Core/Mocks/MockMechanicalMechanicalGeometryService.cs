using Plugin.Core.Interfaces;
using System.Collections.Generic;
using System.Numerics;
using Utility.Mathematics.Shapes;

namespace Plugin.Core.Mocks;

public class MockMechanicalMechanicalGeometryService : IMechanicalGeometryService
{
    public IEnumerable<PipeModel> GetSelectedPipes()
    {
        var pipes = new List<PipeModel>
        {
            new()
            {
                ElementId = 101,
                StartPoint = new Vector3(0, 0, 0),
                EndPoint = new Vector3(2.75f, 0, 0),
                Diameter = 0.2,
            },
            new()
            {
                ElementId = 102,
                StartPoint = new Vector3(3, 0, 0),
                EndPoint = new Vector3(3, 3, 0),
                Diameter = 0.2,
            },
            new()
            {
                ElementId = 103,
                StartPoint = new Vector3(0, 2, 0),
                EndPoint = new Vector3(5, 2, 0),
                Diameter = 0.2,
            },
            new()
            {
                ElementId = 104,
                StartPoint = new Vector3(0, 2, 0.2f),
                EndPoint = new Vector3(5, 2, 0.2f),
                Diameter = 0.2,
            },

            /*
                TopLeft = new Vector3(-10, 05, 10),
                BottomRight = new Vector3(-15, 10, 05),
             */

            new()
            {
                ElementId = 105,
                StartPoint = new Vector3(-15, 05, 7.5f),
                EndPoint = new Vector3(-10, 10, 7.5f),
                Diameter = 0.2,
            },

            new()
            {
                ElementId = 106,
                StartPoint = new Vector3(-13, 03, 7.75f),
                EndPoint = new Vector3(-12, 12, 7.75f),
                Diameter = 0.2,
            },
        };
        return pipes;
    }

    public List<ConnectorModel> GetConnectors()
    {
        var connectors = new List<ConnectorModel>
        {
            new()
            {
                ElementId = 501,
                Position = new Vector3(3, 0, 0),
                Diameter = 0.2,
                ConnectionPoints =
                [
                    new() { Direction = new Vector3(-1, 0, 0), Position = new Vector3(3, 0, 0), Diameter = 0.4, IsOutput = false, ElementId = 501, },
                    new() { Direction = new Vector3(0, 1, 0), Position = new Vector3(3, 0, 0), Diameter = 0.4, IsOutput = true, ElementId = 501,},
                ],
            },
        };
        return connectors;
    }

    public List<WallModel> GetSelectedWallGeometries()
    {
        var result = new List<WallModel>
        {
            new()
            {
                ElementId =701,
                Rectangles =
                [
                    new()
                    {
                        ElementId = 701,
                        TopLeft = new Vector3(00, -5, 10),
                        BottomRight = new Vector3(10, -5, 00),
                        IsOpening = false,
                        Normal = new Vector3(1, 0, 0),
                    },
                    new()
                    {
                        ElementId = 701,
                        TopLeft = new Vector3(03, -5, 03),
                        BottomRight = new Vector3(05, -5, 05),
                        IsOpening = true,
                        Normal = new Vector3(1, 0, 0),
                    },
                ],
            },

            new()
            {
                ElementId =702,
                Rectangles =
                [

                    //left
                    new()
                    {
                        ElementId = 702,
                        TopLeft = new Vector3(-11, 6, 5),
                        BottomRight = new Vector3(-12, 7, 10),
                        IsOpening = false,
                        Normal = new Vector3(1, 0, 0),
                    },

                    //middle
                    new()
                    {
                        ElementId = 702,
                        TopLeft = new Vector3(-12, 07, 07),
                        BottomRight = new Vector3(-13, 08, 5),
                        IsOpening = false,
                        Normal = new Vector3(1, 0, 0),
                    },
                    new()
                    {
                        ElementId = 702,
                        TopLeft = new Vector3(-12, 07, 08),
                        BottomRight = new Vector3(-13, 08, 07),
                        IsOpening = true,
                        Normal = new Vector3(1, 0, 0),
                    },
                    new()
                    {
                        ElementId = 702,
                        TopLeft = new Vector3(-12, 07, 10),
                        BottomRight = new Vector3(-13, 08, 08),
                        IsOpening = false,
                        Normal = new Vector3(1, 0, 0),
                    },

                    //right
                    new()
                    {
                        ElementId = 702,
                        TopLeft = new Vector3(-13, 08, 5),
                        BottomRight = new Vector3(-14, 9, 10),
                        IsOpening = false,
                        Normal = new Vector3(1, 0, 0),
                    },

                ],
            },

            new()
            {
                ElementId =703,
                Rectangles =
                [

                    //left
                    new()
                    {
                        ElementId = 703,
                        TopLeft = new Vector3(-11, 08, 5),
                        BottomRight = new Vector3(-12, 09, 10),
                        IsOpening = false,
                        Normal = new Vector3(1, 0, 0),
                    },

                    //middle
                    new()
                    {
                        ElementId = 703,
                        TopLeft = new Vector3(-12, 09, 07),
                        BottomRight = new Vector3(-13, 10, 5),
                        IsOpening = false,
                        Normal = new Vector3(1, 0, 0),
                    },
                    new()
                    {
                        ElementId = 703,
                        TopLeft = new Vector3(-12, 09, 08),
                        BottomRight = new Vector3(-13, 10, 07),
                        IsOpening = true,
                        Normal = new Vector3(1, 0, 0),
                    },
                    new()
                    {
                        ElementId = 703,
                        TopLeft = new Vector3(-12, 09, 10),
                        BottomRight = new Vector3(-13, 10, 08),
                        IsOpening = false,
                        Normal = new Vector3(1, 0, 0),
                    },

                    //right
                    new()
                    {
                        ElementId = 703,
                        TopLeft = new Vector3(-13, 10, 5),
                        BottomRight = new Vector3(-14, 11, 10),
                        IsOpening = false,
                        Normal = new Vector3(1, 0, 0),
                    },

                ],
            },
        };
        return result;
    }
}