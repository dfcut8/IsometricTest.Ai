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

    public bool TryMoveUnit(UnitState unit, Vector2I destination)
    {
        if (!IsInBounds(destination) || IsOccupied(destination))
            return false;

        _occupants.Remove(unit.GridPosition);
        unit.GridPosition = destination; // Logical state changes before its visual is animated.
        _occupants.Add(destination, unit);
        return true;
    }
}
