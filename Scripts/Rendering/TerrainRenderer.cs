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

        foreach (TerrainType terrain in System.Enum.GetValues(typeof(TerrainType)))
        {
            _sideMaterials[terrain] = TexturedMaterial(PrototypeTextures.LoadTerrainSide(terrain));
            _topMaterials[terrain] = TexturedMaterial(PrototypeTextures.LoadTerrainTop(terrain));
        }

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
        AddSideFace(body, "SideNorth", new Vector3(0, 0, -size.Z * 0.5f - 0.001f), 180, size, cell.Terrain);
        AddSideFace(body, "SideEast", new Vector3(size.X * 0.5f + 0.001f, 0, 0), 90, size, cell.Terrain);
        AddSideFace(body, "SideSouth", new Vector3(0, 0, size.Z * 0.5f + 0.001f), 0, size, cell.Terrain);
        AddSideFace(body, "SideWest", new Vector3(-size.X * 0.5f - 0.001f, 0, 0), -90, size, cell.Terrain);
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

    private void AddSideFace(
        Node3D body,
        string name,
        Vector3 position,
        float rotationY,
        Vector3 tileSize,
        TerrainType terrain)
    {
        body.AddChild(new MeshInstance3D
        {
            Name = name,
            Mesh = new QuadMesh { Size = new Vector2(tileSize.X, tileSize.Y) },
            MaterialOverride = _sideMaterials[terrain],
            Position = position,
            RotationDegrees = new Vector3(0, rotationY, 0)
        });
    }

    private static StandardMaterial3D TexturedMaterial(Texture2D texture) => new()
    {
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        AlbedoTexture = texture,
        TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
        CullMode = BaseMaterial3D.CullModeEnum.Disabled,
        Roughness = 1.0f
    };
}
