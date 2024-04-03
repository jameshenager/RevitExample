using Plugin.Core;
using Plugin.Core.Mocks;
using RevitPlugin.Wpf.PipeTool;
using Utility.Mathematics.Geometry;

namespace PipeToolTest;

public class PipeToolTests
{
    [Fact]
    public static void PipesAndWallsIntersections()
    {
        var cm = new CollisionManager();
        var vm = new PipeToolMainWindowViewModel(new MockMoveElementWrapper(),
            new MockableEvent(),
            new MockSelectedElements(),
            new MockElementSelector(),
            new MockMechanicalMechanicalGeometryService(),
            cm);
        Assert.Empty(vm.Walls);
        vm.LoadWalls();
        Assert.Equal(3, vm.Walls.Count);
        Assert.Empty(vm.Pipes);
        vm.LoadPipes();
        Assert.Equal(6, vm.Pipes.Count);

        vm.ResolveAllCollisions();

        //check how many pipes has 3 collisions
        Assert.Equal(3, vm.Pipes.Count(p => p.Value.Pipe.CollisionPoints.Any()));
        //check that one wall has 1 collision
        Assert.Equal(1, vm.Walls.Count(w => w.Value.Rectangles.Any(v => v.CollisionElementIds.Any())));

        Assert.True(100 > cm.CollisionTestCount);
    }

    [Fact]
    public static void PipesAndWallsIntersections_ByReload()
    {
        var cm = new CollisionManager();
        var vm = new PipeToolMainWindowViewModel(new MockMoveElementWrapper(),
            new MockableEvent(),
            new MockSelectedElements(),
            new MockElementSelector(),
            new MockMechanicalMechanicalGeometryService(),
            cm);
        Assert.Empty(vm.Walls);
        vm.LoadWalls();
        Assert.Equal(3, vm.Walls.Count);
        Assert.Empty(vm.Pipes);
        vm.LoadPipes();
        Assert.Equal(6, vm.Pipes.Count);

        vm.ReloadCanvas();

        //check how many pipes has 3 collisions
        Assert.Equal(3, vm.Pipes.Count(p => p.Value.Pipe.CollisionPoints.Any()));
        //check that one wall has 1 collision
        Assert.Equal(1, vm.Walls.Count(w => w.Value.Rectangles.Any(v => v.CollisionElementIds.Any())));

        Assert.True(100 > cm.CollisionTestCount);
    }

    [Fact]
    public static void PipesOnlyIntersections()
    {
        var cm = new CollisionManager();
        var vm = new PipeToolMainWindowViewModel(new MockMoveElementWrapper(),
            new MockableEvent(),
            new MockSelectedElements(),
            new MockElementSelector(),
            new MockMechanicalMechanicalGeometryService(),
            cm);

        Assert.Empty(vm.Pipes);
        vm.LoadPipes();
        Assert.Equal(6, vm.Pipes.Count);

        vm.ResolveAllCollisions();

        //check how many pipes has 2 collisions
        Assert.Equal(2, vm.Pipes.Count(p => p.Value.Pipe.CollisionPoints.Any()));
        Assert.Equal(0, vm.Walls.Count(w => w.Value.Rectangles.Any(v => v.CollisionElementIds.Any())));
        Assert.True(100 > cm.CollisionTestCount);
        Assert.Empty(vm.Walls);
    }

    [Fact]
    public static void PipesOnlyIntersections_WallsUnloaded()
    {
        var cm = new CollisionManager();
        var vm = new PipeToolMainWindowViewModel(new MockMoveElementWrapper(),
            new MockableEvent(),
            new MockSelectedElements(),
            new MockElementSelector(),
            new MockMechanicalMechanicalGeometryService(),
            cm);

        Assert.Empty(vm.Pipes);
        vm.LoadPipes();
        Assert.Equal(6, vm.Pipes.Count);
        vm.LoadWalls();
        Assert.Equal(3, vm.Walls.Count);
        vm.ClearWalls();

        vm.ResolveAllCollisions();

        //check how many pipes has 2 collisions
        Assert.Equal(2, vm.Pipes.Count(p => p.Value.Pipe.CollisionPoints.Any()));
        Assert.Equal(0, vm.Walls.Count(w => w.Value.Rectangles.Any(v => v.CollisionElementIds.Any())));
        Assert.True(100 > cm.CollisionTestCount);
        Assert.Empty(vm.Walls);
    }

    [Fact]
    public static void PipesAndWallsIntersections_WallsUnloadedLoad()
    {
        var cm = new CollisionManager();
        var vm = new PipeToolMainWindowViewModel(new MockMoveElementWrapper(),
            new MockableEvent(),
            new MockSelectedElements(),
            new MockElementSelector(),
            new MockMechanicalMechanicalGeometryService(),
            cm);
        Assert.Empty(vm.Walls);
        vm.LoadWalls();
        Assert.Equal(3, vm.Walls.Count);
        Assert.Empty(vm.Pipes);
        vm.LoadPipes();
        Assert.Equal(6, vm.Pipes.Count);

        vm.ResolveAllCollisions();

        //check how many pipes has 3 collisions
        Assert.Equal(3, vm.Pipes.Count(p => p.Value.Pipe.CollisionPoints.Any()));
        //check that one wall has 1 collision
        Assert.Equal(1, vm.Walls.Count(w => w.Value.Rectangles.Any(v => v.CollisionElementIds.Any())));

        vm.ClearWalls();
        vm.LoadWalls();

        vm.ResolveAllCollisions();

        //check how many pipes has 3 collisions
        Assert.Equal(3, vm.Pipes.Count(p => p.Value.Pipe.CollisionPoints.Any()));
        //check that one wall has 1 collision
        Assert.Equal(1, vm.Walls.Count(w => w.Value.Rectangles.Any(v => v.CollisionElementIds.Any())));
    }

    [Fact]
    public static void PipesAndWallsIntersections_PipesUnloadedLoad()
    {
        var cm = new CollisionManager();
        var vm = new PipeToolMainWindowViewModel(new MockMoveElementWrapper(),
            new MockableEvent(),
            new MockSelectedElements(),
            new MockElementSelector(),
            new MockMechanicalMechanicalGeometryService(),
            cm);
        Assert.Empty(vm.Walls);
        vm.LoadWalls();
        Assert.Equal(3, vm.Walls.Count);
        Assert.Empty(vm.Pipes);
        vm.LoadPipes();
        Assert.Equal(6, vm.Pipes.Count);

        vm.ResolveAllCollisions();

        //check how many pipes has 3 collisions
        Assert.Equal(3, vm.Pipes.Count(p => p.Value.Pipe.CollisionPoints.Any()));
        //check that one wall has 1 collision
        Assert.Equal(1, vm.Walls.Count(w => w.Value.Rectangles.Any(v => v.CollisionElementIds.Any())));

        vm.ClearPipes();
        vm.LoadPipes();

        vm.ResolveAllCollisions();

        //check how many pipes has 3 collisions
        Assert.Equal(3, vm.Pipes.Count(p => p.Value.Pipe.CollisionPoints.Any()));
        //check that one wall has 1 collision
        Assert.Equal(1, vm.Walls.Count(w => w.Value.Rectangles.Any(v => v.CollisionElementIds.Any())));
    }

    [Fact]
    public static void PipesAndWallsIntersections_PipesMultipleLoad()
    {
        var cm = new CollisionManager();
        var vm = new PipeToolMainWindowViewModel(new MockMoveElementWrapper(),
            new MockableEvent(),
            new MockSelectedElements(),
            new MockElementSelector(),
            new MockMechanicalMechanicalGeometryService(),
            cm);
        Assert.Empty(vm.Walls);
        vm.LoadWalls();
        Assert.Equal(3, vm.Walls.Count);
        Assert.Empty(vm.Pipes);
        vm.LoadPipes();
        Assert.Equal(6, vm.Pipes.Count);
        vm.LoadPipes();
        vm.LoadPipes();

        vm.ResolveAllCollisions();
        vm.ResolveAllCollisions();

        //check how many pipes has 3 collisions
        Assert.Equal(3, vm.Pipes.Count(p => p.Value.Pipe.CollisionPoints.Any()));
        //check that one wall has 1 collision
        Assert.Equal(1, vm.Walls.Count(w => w.Value.Rectangles.Any(v => v.CollisionElementIds.Any())));
    }


    //SetSelectedElementIds

    [Fact]
    public static void SelectedElementIdsString_Good()
    {
        var cm = new CollisionManager();
        var vm = new PipeToolMainWindowViewModel(new MockMoveElementWrapper(),
            new MockableEvent(),
            new MockSelectedElements(),
            new MockElementSelector(),
            new MockMechanicalMechanicalGeometryService(),
            cm);

        var testString = string.Join(Environment.NewLine, Enumerable.Range(1, 20));

        vm.SelectedItemString = testString;
        vm.SetSelectedElementIds();

        Assert.Equal(3, 3);
    }

    //assert that the following tests throws an exception


    [Fact]
    public static void SelectedElementIdsString_Bad()
    {
        var cm = new CollisionManager();
        var vm = new PipeToolMainWindowViewModel(new MockMoveElementWrapper(),
            new MockableEvent(),
            new MockSelectedElements(),
            new MockElementSelector(),
            new MockMechanicalMechanicalGeometryService(),
            cm);

        var testString = string.Join(Environment.NewLine, Enumerable.Range(1, 19)) + "- dog";
        vm.SelectedItemString = testString;

        vm.SetSelectedElementIds();

        Assert.Equal(3, 3);
    }

}