using Godot;

namespace IsometricTestAI.Units;

public sealed class UnitState
{
    public string Name { get; }
    public Vector2I GridPosition { get; internal set; }
    public FacingDirection Facing { get; set; }

    public UnitState(string name, Vector2I gridPosition, FacingDirection facing)
    {
        Name = name;
        GridPosition = gridPosition;
        Facing = facing;
    }
}
