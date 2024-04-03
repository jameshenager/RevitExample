using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelixToolkit.Wpf;
using Plugin.Core;
using Plugin.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Utility.Mathematics.Geometry;
using Utility.Mathematics.Shapes;
using Wpf.Common;

namespace RevitPlugin.Wpf.PipeTool;
//Features to add:

//02. Add a file to save the state so it can all be reloaded.

//04. Add floors and ceilings so I can bound the pipes.
//05. Add a textBox to set the radius of the collision spheres. If it's zero, then don't show it.

//07. Make it identify beams above and try to add supports to the pipe there or every 10 feet or whatever.
//08. Draw curved tubes
//09. Add unit tests
//10. Update Connector collision code
//11. Output all collision elementIds to another textBox

[RevitCommand(1, "My Panel", "My Tab", @"E:\Programming\RevitPlugin\RevitPlugin\RevitPlugin\bin\x64\Debug\spinning world with binoculars.png", "PipeToolWindow", "Pipe Tool")]
public partial class PipeToolMainWindowViewModel(
    IMoveElementWrapper elementMoverEventHandler,
    IMockableEvent mockableEvent,
    IQuerySelectedElements selectElementsEventHandler,
    IElementSelector selectedElementQueryService,
    IMechanicalGeometryService mechanicalGeometryService,
    CollisionManager collisionManager) : ObservableObject /* ObservableObject is still useful for relayCommand */
{
    //[ObservableProperty] private string _selectedItemString; //This breaks the code in .net framework because sourceGen doesn't work yet.

    private Model3D _model;
    public Model3D Model { get => _model; set { if (SetProperty(ref _model, value)) { _ = 1; } } }
    public event Action ZoomExtentsAction = () => { };

    private string _selectedItemString = "";
    public string SelectedItemString { get => _selectedItemString; set => SetProperty(ref _selectedItemString, value); }

    //ToDo: Create a button to unload everything. Also maybe make a file to save the state somewhere so it can all be reloaded.
    //ToDo: Also maybe make it so I can add things to a collection.

    [RelayCommand] public void UserControlLoaded() => selectedElementQueryService.SelectionUpdated += OnSelectionUpdated;
    [RelayCommand] public void UserControlUnloaded() => selectedElementQueryService.SelectionUpdated -= OnSelectionUpdated;
    [RelayCommand] public void ZoomExtents() => ZoomExtentsAction.Invoke();
    [RelayCommand] public void LoadConnections() => Connectors.ReplaceWith(mechanicalGeometryService.GetConnectors());
    public Dictionary<long, WallModel> Walls { get; set; } = [];
    [RelayCommand]
    public void LoadWalls()
    {
        var sw = Stopwatch.StartNew();
        var tempWalls = mechanicalGeometryService.GetSelectedWallGeometries();
        foreach (var wall in tempWalls) { Walls[wall.ElementId] = wall; }
        collisionManager.AddWalls(tempWalls);
        Trace.WriteLine(sw.ElapsedMilliseconds + $" to load {tempWalls.Count} walls data.");
    }

    private void OnSelectionUpdated(List<long> list) => SelectedItemString = string.Join(Environment.NewLine, list) /* for some Revit reason, probably, I don't have to use Application.Current.Dispatcher.Invoke */;
    [RelayCommand] public void GetSelectedElementIds() => SelectedItemString = string.Join(Environment.NewLine, selectElementsEventHandler.GetSelectedElements());

    public Dictionary<long, PipeModelInfo> Pipes { get; set; } = [];
    public ObservableCollectionEx<ConnectorModel> Connectors { get; set; } = [];

    //ToDo: Get walls and ceilings as well so I can bound the pipes
    [RelayCommand] public void ClearPipes() { collisionManager.ClearPipes(); Pipes.Clear(); }
    [RelayCommand] public void ClearWalls() { collisionManager.ClearWalls(); Walls.Clear(); }

    [RelayCommand]
    public void ReloadCanvas()
    {
        var modelGroup = new Model3DGroup();

        //ToDo: Rip out all of the collision code and make it modify the pipes and walls directly.
        DrawRandomBox(modelGroup);
        var sw = Stopwatch.StartNew();
        ResolveAllCollisions();
        Trace.WriteLine(sw.ElapsedMilliseconds + " to resolve all collisions.");
        DrawPipes(modelGroup, Pipes.Select(p => p.Value));
        DrawConnectors(modelGroup, Connectors);
        DrawWalls(modelGroup, Walls.Select(w => w.Value));

        Model = modelGroup;

        Trace.WriteLine($"{collisionManager.CollisionTestCount} collision checks performed");

        /*
        Old
        13005 collision checks performed, 805ms to draw walls with 1692 triangles.

        Half New
        111ms to set up spatial data structure
        752ms to draw walls with 1692 triangles.
        6477 collision checks performed //I can still improve this by using the new data structure in the walls drawing method.

        111 to set up spatial data structure
        935 to draw walls with 1692 triangles.
        1170 collision checks performed

        114 to set up spatial data structure
        70 to draw walls with 1692 triangles.
        487 collision checks performed //but didn't detect some things

        Latest
        269 to set up spatial data structure
        137 to draw walls with 1692 triangles.
        479 collision checks performed

        234 to set up spatial data structure
        120 to draw walls with 1040 triangles. //Took out too many triangles. Some walls were missing.
        519 collision checks performed

        254ms to set up spatial data structure
        83ms to draw walls with 1136 triangles.
        519 collision checks performed

        269 to set up spatial data structure
        62 to draw walls with 568 triangles. //with BackMaterial set to the same as Material
        629 collision checks performed //I added more pipes in walls

        277 to load 20 walls data.
        31 to load 15 pipes data.
        74 to resolve all collisions.
        27 to draw walls with 568 triangles.
        676 collision checks performed
        */
    }

    public void ResolveAllCollisions()
    {
        //ToDo: I should also try to store hashes of elements so I can quickly see if they've actually changed at all. This will help determine whether collisions need to be checked again.
        foreach (var pipe in Pipes.Select(p => p.Value)) { _ = collisionManager.GetPipeCollisions(pipe); }
        foreach (var wall in Walls.Select(w => w.Value)) { _ = collisionManager.GetWallCollisions(wall); }
    }

    private static void DrawConnectors(Model3DGroup modelGroup, IEnumerable<ConnectorModel> connectors)
    {
        var goodConnectorMaterial = MaterialHelper.CreateMaterial(Colors.Green);
        var badConnectorMaterial = MaterialHelper.CreateMaterial(Colors.Red);
        var connectionIdToInformation = new Dictionary<long, ConnectorModelInformation>();

        foreach (var connector in connectors)
        {
            var cmi = new ConnectorModelInformation() { Cylinders = connector.ConnectionPoints.Select(cp => cp.CylinderModel).ToList(), };
            foreach (var otherConnector in connectionIdToInformation)
            {
                foreach (var otherCylinder in otherConnector.Value.Cylinders)
                {
                    if (cmi.Cylinders.Any(c => CollisionHelper.Collides(c, otherCylinder)))
                    {
                        cmi.IntersectConnectorId.Add(otherConnector.Key);
                    }
                }
            }
            connectionIdToInformation.Add(connector.ElementId, cmi);

            var connectorPosition = connector.Position.ToPoint3D();

            var connectorMeshBuilder = new MeshBuilder(false, false);
            connectorMeshBuilder.AddSphere(connectorPosition, connector.Diameter / 2);
            var connectorMesh = connectorMeshBuilder.ToMesh(true);
            modelGroup.Children.Add(new GeometryModel3D
            {
                Geometry = connectorMesh,
                Material = connectionIdToInformation[connector.ElementId].IntersectConnectorId.Any() ? badConnectorMaterial : goodConnectorMaterial,
            });

            foreach (var connectionPoint in connector.ConnectionPoints)
            {
                var directionEndPoint = connector.Position + connectionPoint.Direction;
                var directionMeshBuilder = new MeshBuilder(false, false);

                directionMeshBuilder.AddPipe(connectorPosition, directionEndPoint.ToPoint3D(), connectionPoint.Diameter / 4, connectionPoint.Diameter, 20);
                var directionMesh = directionMeshBuilder.ToMesh(true);
                modelGroup.Children.Add(new GeometryModel3D
                {
                    Geometry = directionMesh,
                    Material = connectionIdToInformation[connector.ElementId].IntersectConnectorId.Any() ? badConnectorMaterial : goodConnectorMaterial,
                });
            }
        }
    }

    private static void DrawPipes(Model3DGroup modelGroup, IEnumerable<PipeModelInfo> pipes)
    {
        var noIntersectionMaterial = MaterialHelper.CreateMaterial(Colors.Gray);
        var pipeIntersectionMaterial = MaterialHelper.CreateMaterial(Colors.Red);

        foreach (var pipe in pipes)
        {
            var pipeMeshBuilder = new MeshBuilder(false, false);
            pipeMeshBuilder.AddPipe(pipe.Pipe.StartPoint.ToPoint3D(), pipe.Pipe.EndPoint.ToPoint3D(), pipe.Pipe.Diameter, pipe.Pipe.Diameter, 20);

            foreach (var cp in pipe.Pipe.CollisionPoints) { pipeMeshBuilder.AddSphere(cp.ToPoint3D(), 0.1, 10, 10); }

            var pipeMesh = pipeMeshBuilder.ToMesh(true);
            modelGroup.Children.Add(new GeometryModel3D { Geometry = pipeMesh, Material = pipe.Pipe.CollisionElementIds.Any() ? pipeIntersectionMaterial : noIntersectionMaterial, });
        }
    }

    private void DrawWalls(Model3DGroup modelGroup, IEnumerable<WallModel> walls)
    {
        var sw = Stopwatch.StartNew();
        var trianglesDrawn = 0;
        foreach (var wall in walls)
        {
            var meshBuilder = new MeshBuilder(false, false);
            foreach (var r in wall.Rectangles.Where(x => !x.IsOpening))
            {
                var triangles = r.GetTwoTriangles();
                foreach (var t in triangles)
                {
                    var points = t.Select(p => p.ToPoint3D()).ToList();
                    meshBuilder.AddTriangle(points[0], points[1], points[2]);
                    trianglesDrawn += 1;
                }
                //ToDo: I should also try to store hashes of elements so I can quickly see if they've actually changed at all. This will help determine whether collisions need to be checked again.
            }

            var hasCollision = wall.Rectangles.Any(w => w.CollisionElementIds.Any());
            foreach (var cp in wall.Rectangles.SelectMany(w => w.CollisionPoints)) { meshBuilder.AddSphere(cp.ToPoint3D(), 0.1, 10, 10); }

            var mesh = meshBuilder.ToMesh(true);
            var color = hasCollision ? Colors.Red : Colors.Green;
            var material = MaterialHelper.CreateMaterial(color, opacity: 0.3);
            modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Material = material, BackMaterial = material, });
        }
        Trace.WriteLine(sw.ElapsedMilliseconds + $" to draw walls with {trianglesDrawn} triangles."); //636 to draw walls with 1692 triangles.
    }

    private static void DrawRandomBox(Model3DGroup modelGroup)
    {
        var boxMeshBuilder = new MeshBuilder(false, false);
        boxMeshBuilder.AddBox(new Point3D(0, 0, 1), 1, 2, 0.5);

        var pointsList = new List<Point3D>() { new(0, 0, 0), new(0, 1, 1), new(0, 1, 6), };
        boxMeshBuilder.AddPolygon(pointsList.ToList());

        var boxMesh = boxMeshBuilder.ToMesh(true);
        var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
        modelGroup.Children.Add(new GeometryModel3D { Geometry = boxMesh, Material = greenMaterial, BackMaterial = greenMaterial, });
    }

    [RelayCommand]
    public void LoadPipes()
    {
        var sw = Stopwatch.StartNew();
        var pipes = mechanicalGeometryService.GetSelectedPipes();
        var checkingDictionary = new Dictionary<long, PipeModelInfo>();
        var pipeResult = new List<PipeModelInfo>();

        foreach (var pipe in pipes)
        {
            var pi = new PipeModelInfo(pipe);
            SetPipeCollisions(checkingDictionary, pi, pipe);
            checkingDictionary.Add(pipe.ElementId, pi);
            pipeResult.Add(pi);
        }
        foreach (var pi in pipeResult) { Pipes[pi.Pipe.ElementId] = pi; }

        collisionManager.AddPipes(pipeResult.Select(pr => pr).ToList());
        Trace.WriteLine(sw.ElapsedMilliseconds + $" to load {pipeResult.Count} pipes data.");
    }

    private void SetPipeCollisions(Dictionary<long, PipeModelInfo> checkingDictionary, PipeModelInfo pi, PipeModel pipe)
    {
        foreach (var connector in checkingDictionary)
        {
            if (!CollisionHelper.Collides(pi.Pipe, connector.Value.Pipe)) { continue; }
            pi.Intersects.Add(connector.Value.Pipe.ElementId);
            connector.Value.Intersects.Add(pipe.ElementId);
        }
    }

    [RelayCommand]
    public void SetSelectedElementIds()
    {
        var elementsToSelect = StringHelper.GetLongs(SelectedItemString);
        selectedElementQueryService.SetSelectedElements(elementsToSelect); // This initiates the set elements and get elements. Completion is handled by the callback to OnSelectionUpdated
    }

    [RelayCommand]
    public void MoveSelectedElements()
    {
        try
        {
            elementMoverEventHandler.ElementIds = SelectedItemString
                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(long.Parse)
                .ToList();
            elementMoverEventHandler.Translation = new Vector3(0, 0, 10);

            mockableEvent.Raise();
        }
        catch (Exception e) { MessageBox.Show(e.Message); }
    }
}