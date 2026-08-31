using Godot;

namespace IsometricTestAI.Map;

public sealed class MapCell
{
    public Vector2I Position { get; set; }
    public TerrainType Terrain { get; set; }
    public int Elevation { get; set; }
}
