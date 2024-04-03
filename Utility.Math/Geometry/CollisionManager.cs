using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Utility.Mathematics.Shapes;

namespace Utility.Mathematics.Geometry;

public class CollisionManager
{
    private readonly Dictionary<Vector3, List<long>> _treeVectorToElementDictionary = [];
    private readonly Dictionary<long, List<Vector3>> _treeElementToVectorDictionary = [];
    private readonly Dictionary<long, PipeModelInfo> _pipeDictionary = [];
    private readonly Dictionary<long, List<RectangleModel>> _wallFaceDictionary = [];
    private readonly Dictionary<long, List<IShape>> _shapesDictionary = [];
    private readonly Dictionary<long, HashSet<long>> _elementsToCheck = [];

    //ToDO: Create a dictionary of IShape ElementId to the other one it should collide with. Always put the one with the lowerId first, then remove from it once those two collide. This way I can remove the pair from the dictionary and not have to check it again.

    public int CollisionTestCount { get; private set; }

    private List<Vector3> GetCollisions(IShape r, IShape p)
    {
        CollisionTestCount++;
        if (!CollisionHelper.Collides(r, p)) { return []; }
        var results = new List<Vector3>();
        foreach (var cp in CollisionHelper.GetCollisionPoints(r, p)) { results.Add(cp); }
        return results;
    }

    public (bool, List<Vector3>, HashSet<long>) GetWallCollisions(WallModel wall)
    {
        var results = new List<Vector3>();
        var collidedElementIds = new HashSet<long>();

        if (!_elementsToCheck.TryGetValue(wall.ElementId, out var thingsToDo)) { return (false, results, collidedElementIds); }

        if (!_wallFaceDictionary.ContainsKey(wall.ElementId)) { return (false, results, collidedElementIds); }
        var rectangleModels = _wallFaceDictionary[wall.ElementId];

        foreach (var thing in thingsToDo)
        {
            if (!_pipeDictionary.ContainsKey(thing)) { continue; }
            var pipeModel = _pipeDictionary[thing];

            foreach (var rectangleModel in rectangleModels.Where(v => !v.IsOpening))
            {
                var collisionPoints = GetCollisions(pipeModel.Pipe, rectangleModel);
                if (!collisionPoints.Any()) { continue; }
                results.AddRange(collisionPoints);
                collidedElementIds.Add(rectangleModel.ElementId);
                collidedElementIds.Add(wall.ElementId);
                rectangleModel.CollisionPoints.AddRange(collisionPoints);
                rectangleModel.CollisionElementIds.Add(pipeModel.Pipe.ElementId);
            }
        }
        return (results.Any(), results, collidedElementIds);
    }

    public (bool, List<Vector3>, HashSet<long>) GetPipeCollisions(PipeModelInfo pipe)
    {
        if (_pipeDictionary.ContainsKey(pipe.Pipe.ElementId) == false) { return (false, [], []); }
        var results = new List<Vector3>();
        var collidedElementIds = new HashSet<long>();

        if (!_elementsToCheck.TryGetValue(pipe.Pipe.ElementId, out var elementsToCheck)) { return (false, results, collidedElementIds); }

        var itemsOne = _pipeDictionary[pipe.Pipe.ElementId];
        foreach (var elementToCheck in elementsToCheck)
        {
            if (!_shapesDictionary.ContainsKey(elementToCheck)) { continue; }

            var itemsTwo = _shapesDictionary[elementToCheck];

            foreach (var two in itemsTwo)
            {
                CollisionTestCount++;
                var collisionPoints = GetCollisions(itemsOne.Pipe, two);
                if (!collisionPoints.Any()) { continue; }
                results.AddRange(collisionPoints);
                collidedElementIds.Add(pipe.Pipe.ElementId);
                collidedElementIds.Add(elementToCheck);

                itemsOne.Pipe.CollisionPoints.AddRange(collisionPoints);
                itemsOne.Pipe.CollisionElementIds.Add(elementToCheck);

                //I could also modify the other shape here and then remove the pair

            }
        }

        return (results.Any(), results, collidedElementIds);
    }

    private void SetupSpatialDataStructure(List<IShape> shapes)
    {
        foreach (var s in shapes)
        {
            if (!_shapesDictionary.ContainsKey(s.ElementId)) { _shapesDictionary[s.ElementId] = []; }
            _shapesDictionary[s.ElementId].Add(s);
        }

        foreach (var shape in shapes)
        {
            var points = shape.GetBoundaryPoints();

            foreach (var point in points)
            {
                if (!_treeVectorToElementDictionary.ContainsKey(point)) { _treeVectorToElementDictionary.Add(point, []); }
                if (!_treeVectorToElementDictionary[point].Contains(shape.ElementId)) { _treeVectorToElementDictionary[point].Add(shape.ElementId); }
            }
            if (!_treeElementToVectorDictionary.ContainsKey(shape.ElementId)) { _treeElementToVectorDictionary.Add(shape.ElementId, []); }
            _treeElementToVectorDictionary[shape.ElementId].AddRange(points);
        }

        foreach (var p in _treeVectorToElementDictionary.Where(d => d.Value.Count > 1))
        {
            foreach (var eId in p.Value)
            {
                if (!_elementsToCheck.ContainsKey(eId)) { _elementsToCheck[eId] = []; }
                foreach (var tId in p.Value.Where(x => x != eId))
                {
                    _elementsToCheck[eId].Add(tId);
                }
            }
        }
    }

    //split this into two, so I can set the pipes and walls separately
    //but this means I need to keep a reference from each shapeId to BoundaryPoints
    //So when I clear walls, I can remove the elementId from the treeVectorToElementDictionary
    //also if that entry has other element Ids, I need to remove the elementId from the elementsToCheck

    public void ClearPipes()
    {
        ClearAllCollisionData();

        foreach (var p in _pipeDictionary.Keys)
        {
            _elementsToCheck[p] = [];
            var elements = _treeElementToVectorDictionary[p];
            foreach (var e in elements)
            {
                if (_treeVectorToElementDictionary.TryGetValue(e, out var value)) { value.Remove(p); }

                if (!_treeVectorToElementDictionary.ContainsKey(e)) { continue; }
                if (_treeVectorToElementDictionary[e].Count == 0) { _treeVectorToElementDictionary.Remove(e); }
                else { foreach (var eId in _treeVectorToElementDictionary[e]) { _elementsToCheck[eId].Remove(p); } }
            }
            _treeElementToVectorDictionary.Remove(p);
            _shapesDictionary.Remove(p);
        }
        _pipeDictionary.Clear();
    }

    public void ClearAllCollisionData()
    {
        foreach (var p in _pipeDictionary.Values)
        {
            p.Pipe.CollisionPoints = [];
            p.Pipe.CollisionElementIds = [];
        }
        foreach (var w in _wallFaceDictionary.Values.SelectMany(v => v))
        {
            w.CollisionPoints = [];
            w.CollisionElementIds = [];
        }
    }

    public void ClearWalls()
    {
        ClearAllCollisionData();
        foreach (var w in _wallFaceDictionary.Keys)
        {
            _elementsToCheck[w] = [];
            var elements = _treeElementToVectorDictionary[w];
            foreach (var e in elements)
            {
                if (_treeVectorToElementDictionary.TryGetValue(e, out var value)) { value.Remove(w); }
                if (!_treeVectorToElementDictionary.ContainsKey(e)) { continue; }
                if (_treeVectorToElementDictionary[e].Count == 0) { _treeVectorToElementDictionary.Remove(e); }
                else { foreach (var eId in _treeVectorToElementDictionary[e]) { _elementsToCheck[eId].Remove(w); } }
            }
            _treeElementToVectorDictionary.Remove(w);
            _shapesDictionary.Remove(w);
        }
        _wallFaceDictionary.Clear();
    }

    public void AddWalls(List<WallModel> walls)
    {
        foreach (var wall in walls)
        {
            if (_wallFaceDictionary.ContainsKey(wall.ElementId)) { _wallFaceDictionary[wall.ElementId] = []; } else { _wallFaceDictionary.Add(wall.ElementId, []); }
            _wallFaceDictionary[wall.ElementId].AddRange(wall.Rectangles.Where(r => !r.IsOpening));
        }
        SetupSpatialDataStructure(walls.SelectMany(w => w.Rectangles.Where(r => !r.IsOpening)).Cast<IShape>().ToList());
    }

    public void AddPipes(List<PipeModelInfo> pipes)
    {
        foreach (var pipe in pipes) { _pipeDictionary[pipe.Pipe.ElementId] = pipe; }
        SetupSpatialDataStructure(pipes.Select(p => p.Pipe).Cast<IShape>().ToList());
    }
}