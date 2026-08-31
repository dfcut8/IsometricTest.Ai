using System.Collections.Generic;
using Godot;
using IsometricTestAI.Map;

namespace IsometricTestAI.Rendering;

public partial class TerrainRenderer : Node3D
{
    private readonly Dictionary<TerrainType, Material> _sideMaterials = new();
    private readonly Dictionary<TerrainType, Material> _topMaterials = new();

    public void Build(TacticalMap map)
    {
        Name = "Terrain";
        _sideMaterials[TerrainType.Grass] = FlatMaterial(new Color("3f5934"));
        _sideMaterials[TerrainType.Dirt] = FlatMaterial(new Color("76502f"));
        _sideMaterials[TerrainType.Rock] = FlatMaterial(new Color("4a5550"));

        foreach (TerrainType terrain in System.Enum.GetValues(typeof(TerrainType)))
            _topMaterials[terrain] = TexturedTopMaterial(PrototypeTextures.LoadTerrainTop(terrain));

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
            MaterialOverride = _sideMaterials[cell.Terrain]
        });
        body.AddChild(new MeshInstance3D
        {
            Name = "TexturedTop",
            Mesh = new PlaneMesh { Size = new Vector2(0.92f, 0.92f) },
            MaterialOverride = _topMaterials[cell.Terrain],
            Position = new Vector3(0, height * 0.5f + 0.003f, 0),
            RotationDegrees = new Vector3(0, ((cell.Position.X * 37 + cell.Position.Y * 17) % 4) * 90, 0)
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

    private static StandardMaterial3D TexturedTopMaterial(Texture2D texture) => new()
    {
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        AlbedoTexture = texture,
        TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
        Roughness = 1.0f
    };
}
