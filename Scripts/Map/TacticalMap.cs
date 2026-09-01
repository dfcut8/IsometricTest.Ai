using System.Collections.Generic;
using Godot;
using IsometricTestAI.Units;

namespace IsometricTestAI.Map;

public sealed class TacticalMap
{
    public const int Width = 10;
    public const int Height = 10;
    public const float ElevationStep = 0.65f;

    public MapCell[,] Cells { get; } = new MapCell[Width, Height];
    private readonly Dictionary<Vector2I, UnitState> _occupants = new();

    public TacticalMap()
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            var dirt = (x is >= 2 and <= 6 && y is >= 2 and <= 4) || (x == 5 && y == 1);
            var rock = x is >= 1 and <= 2 && y is >= 5 and <= 6;
            var elevation = x is >= 6 and <= 8 && y is >= 6 and <= 8 ? 1 : 0;
            if (x == 8 && y == 8)
                elevation = 2;

            Cells[x, y] = new MapCell
            {
                Position = new Vector2I(x, y),
                Terrain = rock ? TerrainType.Rock : dirt ? TerrainType.Dirt : TerrainType.Grass,
                Elevation = elevation
            };
        }
    }

    public bool IsInBounds(Vector2I cell) =>
        cell.X >= 0 && cell.X < Width && cell.Y >= 0 && cell.Y < Height;

    public MapCell GetCell(Vector2I cell) => Cells[cell.X, cell.Y];

    public void SetWalkable(Vector2I cell, bool walkable)
    {
        if (IsInBounds(cell))
            GetCell(cell).IsWalkable = walkable;
    }

    public Vector3 GridToWorld(Vector2I cell)
    {
        var elevation = GetCell(cell).Elevation * ElevationStep;
        return new Vector3(cell.X, elevation, cell.Y);
    }

    public bool AddUnit(UnitState unit)
    {
        if (!IsInBounds(unit.GridPosition) || _occupants.ContainsKey(unit.GridPosition))
            return false;
        _occupants.Add(unit.GridPosition, unit);
        return true;
    }

    public bool IsOccupied(Vector2I cell) => _occupants.ContainsKey(cell);

    public List<Vector2I> FindPath(UnitState unit, Vector2I destination)
    {
        var start = unit.GridPosition;
        if (!IsInBounds(destination) || !GetCell(destination).IsWalkable)
            return new List<Vector2I>();
        if (destination != start && IsOccupied(destination))
            return new List<Vector2I>();

        var frontier = new PriorityQueue<Vector2I, int>();
        var cameFrom = new Dictionary<Vector2I, Vector2I>();
        var costSoFar = new Dictionary<Vector2I, int> { [start] = 0 };
        frontier.Enqueue(start, 0);

        while (frontier.TryDequeue(out var current, out _))
        {
            if (current == destination)
                return ReconstructPath(cameFrom, start, destination);

            foreach (var next in GetNeighbors(current, unit))
            {
                var nextCost = costSoFar[current] + 1;
                if (costSoFar.TryGetValue(next, out var existingCost) && nextCost >= existingCost)
                    continue;

                costSoFar[next] = nextCost;
                cameFrom[next] = current;
                var priority = nextCost + ManhattanDistance(next, destination);
                frontier.Enqueue(next, priority);
            }
        }

        return new List<Vector2I>();
    }

    public bool TryMoveUnit(UnitState unit, Vector2I destination)
    {
        if (!CanEnter(unit, destination))
            return false;

        _occupants.Remove(unit.GridPosition);
        unit.GridPosition = destination; // Logical state changes before its visual is animated.
        _occupants.Add(destination, unit);
        return true;
    }

    private IEnumerable<Vector2I> GetNeighbors(Vector2I cell, UnitState unit)
    {
        // Fixed order keeps equally short previews stable as the mouse moves.
        Vector2I[] directions =
        {
            Vector2I.Right,
            Vector2I.Down,
            Vector2I.Left,
            Vector2I.Up
        };

        foreach (var direction in directions)
        {
            var next = cell + direction;
            if (CanEnterFrom(unit, cell, next))
                yield return next;
        }
    }

    private bool CanEnter(UnitState unit, Vector2I destination) =>
        CanEnterFrom(unit, unit.GridPosition, destination);

    private bool CanEnterFrom(UnitState unit, Vector2I origin, Vector2I destination)
    {
        if (!IsInBounds(origin) || !IsInBounds(destination) || !GetCell(destination).IsWalkable)
            return false;
        if (destination != unit.GridPosition && IsOccupied(destination))
            return false;

        return Mathf.Abs(GetCell(destination).Elevation - GetCell(origin).Elevation) <= 1;
    }

    private static int ManhattanDistance(Vector2I from, Vector2I to) =>
        Mathf.Abs(from.X - to.X) + Mathf.Abs(from.Y - to.Y);

    private static List<Vector2I> ReconstructPath(
        IReadOnlyDictionary<Vector2I, Vector2I> cameFrom,
        Vector2I start,
        Vector2I destination)
    {
        var path = new List<Vector2I> { destination };
        var current = destination;
        while (current != start)
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}
