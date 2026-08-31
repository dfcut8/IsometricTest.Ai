using System.Collections.Generic;
using Godot;
using IsometricTestAI.Map;

namespace IsometricTestAI.Rendering;

public partial class TerrainRenderer : Node3D
{
    private readonly Dictionary<TerrainType, Material> _materials = new();

    public void Build(TacticalMap map)
    {
        Name = "Terrain";
        _materials[TerrainType.Grass] = FlatMaterial(new Color("668f50"));
        _materials[TerrainType.Dirt] = FlatMaterial(new Color("a87748"));
        _materials[TerrainType.Rock] = FlatMaterial(new Color("707a86"));

        for (var x = 0; x < TacticalMap.Width; x++)
        for (var y = 0; y < TacticalMap.Height; y++)
            AddTile(map.Cells[x, y]);
    }

    private void AddTile(MapCell cell)
    {
        const float baseY = -0.34f;
        var topY = cell.Elevation * TacticalMap.ElevationStep;
        var height = topY - baseY;
        var body = new StaticBody3D
        {
            Name = $"Tile_{cell.Position.X}_{cell.Position.Y}",
            Position = new Vector3(cell.Position.X, baseY + height * 0.5f, cell.Position.Y),
            CollisionLayer = 1,
            CollisionMask = 0
        };
        body.SetMeta("grid_position", cell.Position);
        AddChild(body);

        var size = new Vector3(0.96f, height, 0.96f);
        body.AddChild(new MeshInstance3D
        {
            Name = "Mesh",
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = _materials[cell.Terrain]
        });
        body.AddChild(new CollisionShape3D
        {
            Name = "Collider",
            Shape = new BoxShape3D { Size = size }
        });
    }

    private static StandardMaterial3D FlatMaterial(Color color) => new()
    {
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        AlbedoColor = color,
        Roughness = 1.0f
    };
}
