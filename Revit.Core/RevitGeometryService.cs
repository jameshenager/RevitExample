using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Plugin.Core;
using Plugin.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Utility.Mathematics.Extensions;
using Utility.Mathematics.Shapes;
using Line = Autodesk.Revit.DB.Line;

namespace Revit.Core;

/* Geometry query only */
/*
Instead of what I'm currently doing, I can just grab edge Faces.CurveLoops.
Holes will have their own CurveLoop
 */
public class RevitGeometryService(UIApplication uiApplication) : IMechanicalGeometryService
{
    public List<ConnectorModel> GetConnectors()
    {
        var doc = uiApplication.ActiveUIDocument.Document;
        var selection = uiApplication.ActiveUIDocument.Selection.GetElementIds();
        var connectors = new List<ConnectorModel>();

        foreach (var id in selection)
        {
            var element = doc.GetElement(id);
            if (element.Category.Id.Value != (int)BuiltInCategory.OST_PipeFitting) continue;

            if (element is not FamilyInstance fitting) { continue; }
            var connectorSet = fitting.MEPModel.ConnectorManager.Connectors;
            var connectorModel = new ConnectorModel
            {
                ElementId = element.Id.Value,
                Position = fitting.Location.ToVector3(),
                Diameter = 0, // Initialize to 0, will be set per connector
                ConnectionPoints = [],
            };

            foreach (Connector connector in connectorSet)
            {
                // Calculate the direction based on the connector's CoordinateSystem's BasisZ vector, which points in the direction of the connector's normal
                var direction = connector.CoordinateSystem.BasisZ;
                var position = connector.Origin;
                var diameter = connector.Radius * 2; // Assuming diameter is twice the radius for circular connectors

                connectorModel.ConnectionPoints.Add(new ConnectionPoint
                {
                    ElementId = connector.Owner.Id.Value,
                    Direction = new Vector3((float)direction.X, (float)direction.Y, (float)direction.Z),
                    Position = new Vector3((float)position.X, (float)position.Y, (float)position.Z),
                    Diameter = diameter,
                    IsOutput = connector.Direction == FlowDirectionType.Out, // Determine if it's an output based on the connector's FlowDirection
                });
            }

            if (connectorModel.ConnectionPoints.Any()) { connectors.Add(connectorModel); }
        }

        return connectors;
    }

    public IEnumerable<PipeModel> GetSelectedPipes()
    {
        var doc = uiApplication.ActiveUIDocument.Document;
        var selection = uiApplication.ActiveUIDocument.Selection.GetElementIds();
        var pipes = new List<PipeModel>();

        foreach (var id in selection)
        {
            var element = doc.GetElement(id);
            if (element is not MEPCurve) { continue; }

            if (element.Location is not LocationCurve locationCurve) { continue; }

            var curve = locationCurve.Curve;
            if (curve is not Line line) { continue; } /* We're only interested in straight pipes/conduits for now */
            var start = line.GetEndPoint(0);
            var end = line.GetEndPoint(1);

            var diameter = element.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER)?.AsDouble() ?? 1d;

            pipes.Add(new PipeModel
            {
                ElementId = element.Id.Value,
                StartPoint = new Vector3((float)start.X, (float)start.Y, (float)start.Z),
                EndPoint = new Vector3((float)end.X, (float)end.Y, (float)end.Z),
                Diameter = diameter,
            });
        }

        return pipes;
    }

    public List<WallModel> GetSelectedWallGeometries()
    {
        var doc = uiApplication.ActiveUIDocument.Document;
        var selection = uiApplication.ActiveUIDocument.Selection.GetElementIds();

        var walls = new FilteredElementCollector(doc)
            .OfClass(typeof(Wall))
            .Where(w => selection.Contains(w.Id))
            .Cast<Wall>()
            .Select(w => new WallModel
            {
                ElementId = w.Id.Value,
                Rectangles = GetWallFaces(w, doc),
            });
        return walls.ToList();
    }

    private static List<RectangleModel> GetWallFaces(HostObject selectedWall, Document doc)
    {
        var elementId = selectedWall.Id.Value;
        var result = new List<RectangleModel>();

        const int numberOfRectangleModelsToGet = 2;
        var biggestWallRectangles = GetRectangleModelsFromWall(selectedWall, numberOfRectangleModelsToGet);

        var openingRectangles = GetOpeningRectanglesFromWall(doc, selectedWall, biggestWallRectangles);

        foreach (var biggestFace in biggestWallRectangles)
        {
            var sortedX = new List<float>();
            var sortedY = new List<float>();
            var sortedZ = new List<float>();

            SetSortedPoints(biggestFace, openingRectangles, sortedX, sortedY, sortedZ);

            var indicesToPointsDictionary = GetAllGridPointsOnPlane(ref sortedX, ref sortedY, ref sortedZ, biggestFace);
            var rectangles = SetRectangleModelsFromPointsDictionary(sortedX, sortedY, sortedZ, indicesToPointsDictionary, biggestFace, elementId);

            foreach (var rectangle in rectangles)
            {
                var center = rectangle.GetCenter();
                foreach (var opening in openingRectangles)
                {
                    if (opening.ContainsPoint(center)) { rectangle.IsOpening = true; }
                }
            }

            CombineHorizontalRectangles(rectangles, sortedX, sortedY);
            result.AddRange(rectangles);
        }
        return result;
    }

    private static void CombineHorizontalRectangles(IList<RectangleModel> rectangles, IList<float> sortedX, IList<float> sortedY)
    {
        var rectanglesCombined = true;
        //var removed = 0;
        while (rectanglesCombined)
        {
            rectanglesCombined = false;
            for (var index = 0; index < rectangles.Count; index++)
            {
                for (var i = 0; i < rectangles.Count; i++)
                {
                    if (i == index) { continue; }
                    var rectangle = rectangles[index];
                    var otherRectangle = rectangles[i];
                    //check top and bottom z values
                    var sameXy = Math.Abs(rectangle.BottomRight.X - otherRectangle.TopLeft.X) < 0.001f && Math.Abs(rectangle.BottomRight.Y - otherRectangle.TopLeft.Y) < 0.001f;
                    var sameZ = Math.Abs(rectangle.TopLeft.Z - otherRectangle.TopLeft.Z) < 0.001f && Math.Abs(rectangle.BottomRight.Z - otherRectangle.BottomRight.Z) < 0.001f;
                    //make sure that they are both openings or both not openings
                    if (rectangle.IsOpening != otherRectangle.IsOpening) { continue; }
                    //Make sure either the X values are the same or consecutive in the sortedX list
                    var xIndex = sortedX.IndexOf(rectangle.BottomRight.X);
                    var otherXIndex = sortedX.IndexOf(otherRectangle.BottomRight.X);
                    if (xIndex != otherXIndex && Math.Abs(xIndex - otherXIndex) != 1) { continue; }
                    //Make sure either the Y values are the same or consecutive in the sortedY list
                    var yIndex = sortedY.IndexOf(rectangle.BottomRight.Y);
                    var otherYIndex = sortedY.IndexOf(otherRectangle.BottomRight.Y);
                    if (yIndex != otherYIndex && Math.Abs(yIndex - otherYIndex) != 1) { continue; }
                    //but make sure both of them aren't the same
                    if (yIndex == otherYIndex && xIndex == otherXIndex) { continue; }
                    //also make sure that the x/y differences are of the same sign
                    var rectangleXDiff = rectangle.BottomRight.X - rectangle.TopLeft.X;
                    var otherRectangleXDiff = otherRectangle.BottomRight.X - otherRectangle.TopLeft.X;
                    var rectangleYDiff = rectangle.BottomRight.Y - rectangle.TopLeft.Y;
                    var otherRectangleYDiff = otherRectangle.BottomRight.Y - otherRectangle.TopLeft.Y;
                    if (Math.Sign(rectangleXDiff) != Math.Sign(otherRectangleXDiff) || Math.Sign(rectangleYDiff) != Math.Sign(otherRectangleYDiff)) { continue; }

                    if (!sameXy || !sameZ) { continue; }
                    //Trace.WriteLine($"Combining rectangles({index}) {rectangle.TopLeft}-{rectangle.BottomRight} and rectangle{i} {otherRectangle.TopLeft}-{otherRectangle.BottomRight}");
                    rectanglesCombined = true;
                    //removed++;
                    rectangle.BottomRight = otherRectangle.BottomRight;
                    rectangles.RemoveAt(i);
                    //Trace.WriteLine($"Combining rectangles, resulting in {rectangle.TopLeft}-{rectangle.BottomRight}");
                    break;
                }

            }
        }
        //if (removed > 0) { Trace.WriteLine($"Removed {removed} rectangles from Element: {rectangles.First().ElementId}"); }
    }

    private static void SetSortedPoints(RectangleModel rectangle, List<RectangleModel> openingRectangles, List<float> sortedX, List<float> sortedY, List<float> sortedZ)
    {
        var uniqueXs = new HashSet<float>();
        var uniqueYs = new HashSet<float>();
        var uniqueZs = new HashSet<float>();

        uniqueXs.Add(rectangle.TopLeft.X);
        uniqueXs.Add(rectangle.BottomRight.X);
        uniqueYs.Add(rectangle.TopLeft.Y);
        uniqueYs.Add(rectangle.BottomRight.Y);
        uniqueZs.Add(rectangle.TopLeft.Z);
        uniqueZs.Add(rectangle.BottomRight.Z);
        foreach (var o in openingRectangles)
        {
            uniqueXs.Add(o.TopLeft.X);
            uniqueXs.Add(o.BottomRight.X);
            uniqueYs.Add(o.TopLeft.Y);
            uniqueYs.Add(o.BottomRight.Y);
            uniqueZs.Add(o.TopLeft.Z);
            uniqueZs.Add(o.BottomRight.Z);
        }

        sortedX.AddRange(uniqueXs.OrderBy(x => x));
        sortedY.AddRange(uniqueYs.OrderBy(y => y));
        sortedZ.AddRange(uniqueZs.OrderBy(z => z));
    }

    private static List<RectangleModel> GetOpeningRectanglesFromWall(Document doc, HostObject selectedWall, IReadOnlyCollection<RectangleModel> biggestWallRectangles)
    {
        var options = new Options { ComputeReferences = true, IncludeNonVisibleObjects = true, };

        var openingRectangleModel = new List<RectangleModel>();
        var elementIds = selectedWall.FindInserts(true, false, false, false);
        var openingsId = elementIds.Select(o => o.Value).ToList();

        var openings = new FilteredElementCollector(doc)
            .OfClass(typeof(Opening))
            .Where(w => openingsId.Contains(w.Id.Value))
            .Cast<Opening>();

        foreach (var opening in openings)
        {
            var elementId = opening.Id.Value;
            var openingGeometry = opening.get_Geometry(options);
            foreach (var obj in openingGeometry)
            {
                if (obj is not Solid solid || solid.Faces.Size <= 0 || !(solid.Volume > 0)) { continue; }

                foreach (Face openingFace in solid.Faces)
                {
                    var openingPlanarFace = openingFace as PlanarFace;
                    if (openingPlanarFace == null) { continue; }

                    var isCoplanar = biggestWallRectangles.Any(wr => Math.Abs(Vector3.Dot(openingPlanarFace.FaceNormal.ToVector3(), Vector3.Normalize(wr.Normal))) > 0.9);

                    if (isCoplanar) { openingRectangleModel.AddRange(WallFaceToRectangleModel(openingFace, elementId, true)); }
                }
            }
        }

        return openingRectangleModel;
    }

    private static List<RectangleModel> GetRectangleModelsFromWall(Element selectedWall, int numberOfRectangleModelsToGet)
    {
        var elementId = selectedWall.Id.Value;
        var result = new List<RectangleModel>();
        var options = new Options { ComputeReferences = true, IncludeNonVisibleObjects = true, };
        var geometryElement = selectedWall.get_Geometry(options);
        Solid wallSolid = null;
        foreach (var obj in geometryElement) { if (obj is not Solid solid || solid.Faces.Size <= 0 || !(solid.Volume > 0)) { continue; } wallSolid = solid; }
        var tempFaces = new List<Face>();
        foreach (Face face in wallSolid?.Faces ?? new()) { if (face is PlanarFace) { tempFaces.Add(face); } }
        var biggestFaces = tempFaces.OrderByDescending(f => f.Area).Take(numberOfRectangleModelsToGet).ToList();
        foreach (var biggestFace in biggestFaces) { var faceModels = WallFaceToRectangleModel(biggestFace, elementId); result.AddRange(faceModels); }

        return result;
    }

    private static List<RectangleModel> SetRectangleModelsFromPointsDictionary(IReadOnlyCollection<float> sortedX, IReadOnlyCollection<float> sortedY, IReadOnlyCollection<float> sortedZ, IReadOnlyDictionary<(int i, int j, int k), Vector3> indicesToPointsDictionary, RectangleModel biggestFace, long elementId)
    {
        var rectangles = new List<RectangleModel>();
        for (var i = 0; i < sortedX.Count; i++)
        {
            //This approach won't work for slanted walls. I could double the loops again for - x, but it's not a good approach
            //ToDo: I could also detect when this one works, and if it does, just run this one. If it doesn't, run the other one.
            for (var j = 0; j < sortedY.Count; j++)
            {
                for (var k = 0; k < sortedZ.Count; k++)
                {
                    if (!indicesToPointsDictionary.ContainsKey((i, j, k))) { continue; }
                    var point1 = indicesToPointsDictionary[(i, j, k)];
                    foreach (var (x, y, z) in IndexCombinations())
                    {
                        if (!indicesToPointsDictionary.ContainsKey((i + x, j + y, k + z))) { continue; }
                        var point2 = indicesToPointsDictionary[(i + x, j + y, k + z)];
                        var rectangle = new RectangleModel
                        {
                            ElementId = elementId,
                            TopLeft = new Vector3(point1.X, point1.Y, point1.Z),
                            BottomRight = new Vector3(point2.X, point2.Y, point2.Z),
                            Normal = biggestFace.Normal,
                            Origin = biggestFace.Origin,
                            IsOpening = false,
                        };
                        rectangles.Add(rectangle);
                    }
                    foreach (var (x, y, z) in IndexCombinations())
                    {
                        if (!indicesToPointsDictionary.ContainsKey((i + x, j - y, k + z))) { continue; }
                        var point2 = indicesToPointsDictionary[(i + x, j - y, k + z)];
                        var rectangle = new RectangleModel
                        {
                            ElementId = elementId,
                            TopLeft = new Vector3(point1.X, point1.Y, point1.Z),
                            BottomRight = new Vector3(point2.X, point2.Y, point2.Z),
                            Normal = biggestFace.Normal,
                            Origin = biggestFace.Origin,
                            IsOpening = false,
                        };
                        rectangles.Add(rectangle);
                    }
                }
            }
            for (var j = sortedY.Count - 1; j >= 0; j--)
            {
                for (var k = 0; k < sortedZ.Count; k++)
                {
                    if (!indicesToPointsDictionary.ContainsKey((i, j, k))) { continue; }
                    var point1 = indicesToPointsDictionary[(i, j, k)];
                    foreach (var (x, y, z) in IndexCombinations())
                    {
                        if (!indicesToPointsDictionary.ContainsKey((i + x, j + y, k + z))) { continue; }
                        if (!indicesToPointsDictionary.ContainsKey((i + x, j + y, k + z))) { continue; }
                        var point2 = indicesToPointsDictionary[(i + x, j + y, k + z)];
                        var rectangle = new RectangleModel
                        {
                            ElementId = elementId,
                            TopLeft = new Vector3(point1.X, point1.Y, point1.Z),
                            BottomRight = new Vector3(point2.X, point2.Y, point2.Z),
                            Normal = biggestFace.Normal,
                            Origin = biggestFace.Origin,
                            IsOpening = false,
                        };
                        rectangles.Add(rectangle);
                    }
                    foreach (var (x, y, z) in IndexCombinations())
                    {
                        if (!indicesToPointsDictionary.ContainsKey((i + x, j - y, k + z))) { continue; }

                        var point2 = indicesToPointsDictionary[(i + x, j - y, k + z)];
                        var rectangle = new RectangleModel
                        {
                            ElementId = elementId,
                            TopLeft = new Vector3(point1.X, point1.Y, point1.Z),
                            BottomRight = new Vector3(point2.X, point2.Y, point2.Z),
                            Normal = biggestFace.Normal,
                            Origin = biggestFace.Origin,
                            IsOpening = false,
                        };
                        rectangles.Add(rectangle);
                    }
                }
            }
        }

        return rectangles;
    }

    private static readonly List<(int x, int y, int z)> IndexCombos = [(0, 1, 1), (1, 0, 1), (1, 1, 1),];
    private static ReadOnlyCollection<(int x, int y, int z)> IndexCombinations() => new(IndexCombos);
    private static Dictionary<(int i, int j, int k), Vector3> GetAllGridPointsOnPlane(ref List<float> sortedX, ref List<float> sortedY, ref List<float> sortedZ, RectangleModel biggestFace)
    {
        var result = new Dictionary<(int i, int j, int k), Vector3>();
        for (var i = 0; i < sortedX.Count; i++)
        {
            var x = sortedX[i];
            for (var j = 0; j < sortedY.Count; j++)
            {
                var y = sortedY[j];
                for (var k = 0; k < sortedZ.Count; k++)
                {
                    var z = sortedZ[k];
                    var point = new Vector3(x, y, z);
                    if (IsPointOnPlane(point, biggestFace.Normal, biggestFace.Origin)) { result.Add((i, j, k), point); }
                }
            }
        }

        var sortedXCopy = new List<float>(sortedX);
        var sortedYCopy = new List<float>(sortedY);
        var sortedZCopy = new List<float>(sortedZ);
        var copyDictionary = new Dictionary<(int i, int j, int k), Vector3>();

        for (var i = sortedX.Count - 1; i >= 0; i--) { if (result.Keys.Any(k => k.i == i)) { continue; } sortedXCopy.RemoveAt(i); }
        for (var i = sortedY.Count - 1; i >= 0; i--) { if (result.Keys.Any(k => k.j == i)) { continue; } sortedYCopy.RemoveAt(i); }
        for (var i = sortedZ.Count - 1; i >= 0; i--) { if (result.Keys.Any(k => k.k == i)) { continue; } sortedZCopy.RemoveAt(i); }

        foreach (var v in result.Values)
        {
            var i = sortedXCopy.IndexOf(v.X);
            var j = sortedYCopy.IndexOf(v.Y);
            var k = sortedZCopy.IndexOf(v.Z);
            copyDictionary.Add((i, j, k), (v));
        }

        sortedX = [.. sortedXCopy,];
        sortedY = [.. sortedYCopy,];
        sortedZ = [.. sortedZCopy,];

        return copyDictionary;
    }

    private static bool IsPointOnPlane(Vector3 point, Vector3 planeNormal, Vector3 planeOrigin, double tolerance = 1e-3) => Math.Abs(Vector3.Dot(planeNormal, point - planeOrigin)) < tolerance;

    // This doesn't work with slanted walls or floors, since the point where min x and min y occur might coincide with the highest z.
    private static IEnumerable<RectangleModel> WallFaceToRectangleModel(Face face, long elementId, bool isOpening = false)
    {
        var rectangles = new List<RectangleModel>();
        var curveLoops = face.GetEdgesAsCurveLoops();
        foreach (var cLoop in curveLoops)
        {
            var vertices = cLoop.SelectMany(l => new[] { l.GetEndPoint(0).ToVector3(), l.GetEndPoint(1).ToVector3(), }).ToList();

            var (topLeft, bottomRight) = vertices.GetMinMax();

            var normalVector = face is PlanarFace planar ? planar.FaceNormal.ToVector3() : face.ComputeNormal(new UV(0, 0)).ToVector3();

            var rectangleModel = new RectangleModel
            {
                ElementId = elementId,
                TopLeft = topLeft,
                BottomRight = bottomRight,
                Normal = normalVector,
                IsOpening = isOpening,
                Origin = (topLeft + bottomRight) / 2,
            };

            rectangles.Add(rectangleModel);
        }
        return rectangles;
    }
}