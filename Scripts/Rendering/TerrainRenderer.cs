using System;
using System.Collections.Generic;
using Godot;
using IsometricTestAI.Map;

namespace IsometricTestAI.Rendering;

public partial class TerrainRenderer : GridMap
{
    // Fill the GridMap cell exactly. Smaller faces leave real gaps that expose
    // internal walls while the camera rotates, making the board flicker apart.
    private const float TileWidth = 1.0f;
    private const float TopWidth = TileWidth;
    private const float BaseY = -0.34f;

    private readonly Dictionary<TerrainType, Material> _sideMaterials = new();
    private readonly Dictionary<TerrainType, Material> _topMaterials = new();
    private readonly Dictionary<(TerrainType Terrain, int Elevation), int> _items = new();

    public void Build(TacticalMap map)
    {
        Name = "Terrain";
        CellSize = new Vector3(1.0f, TacticalMap.ElevationStep, 1.0f);
        // GridMap centers each item within its cell. Shift the whole grid back by
        // half a cell so logical coordinates remain the world-space tile centers.
        Position = -CellSize * 0.5f;
        CollisionLayer = 1;
        CollisionMask = 0;

        foreach (TerrainType terrain in Enum.GetValues(typeof(TerrainType)))
        {
            _sideMaterials[terrain] = TexturedMaterial(PrototypeTextures.LoadTerrainSide(terrain));
            _topMaterials[terrain] = TexturedMaterial(PrototypeTextures.LoadTerrainTop(terrain));
        }

        MeshLibrary = BuildMeshLibrary(map);

        for (var x = 0; x < TacticalMap.Width; x++)
        for (var z = 0; z < TacticalMap.Height; z++)
        {
            var cell = map.Cells[x, z];
            var quarterTurns = (cell.Position.X * 37 + cell.Position.Y * 17) % 4;
            var orientation = GetOrthogonalIndexFromBasis(
                new Basis(Vector3.Up, Mathf.Pi * 0.5f * quarterTurns));
            SetCellItem(
                new Vector3I(x, cell.Elevation, z),
                _items[(cell.Terrain, cell.Elevation)],
                orientation);
        }
    }

    public Vector2I CellFromWorldPosition(Vector3 worldPosition)
    {
        var gridCell = LocalToMap(ToLocal(worldPosition));
        return new Vector2I(gridCell.X, gridCell.Z);
    }

    private MeshLibrary BuildMeshLibrary(TacticalMap map)
    {
        var library = new MeshLibrary();
        var nextItemId = 0;

        for (var x = 0; x < TacticalMap.Width; x++)
        for (var z = 0; z < TacticalMap.Height; z++)
        {
            var cell = map.Cells[x, z];
            var key = (cell.Terrain, cell.Elevation);
            if (_items.ContainsKey(key))
                continue;

            var itemId = nextItemId++;
            _items.Add(key, itemId);
            library.CreateItem(itemId);
            library.SetItemName(itemId, $"{cell.Terrain}_{cell.Elevation}");
            library.SetItemMesh(itemId, CreateTileMesh(cell.Terrain, cell.Elevation));

            var height = cell.Elevation * TacticalMap.ElevationStep - BaseY;
            var collision = new BoxShape3D
            {
                Size = new Vector3(TileWidth, height, TileWidth)
            };
            library.SetItemShapes(itemId, new Godot.Collections.Array
            {
                collision,
                new Transform3D(Basis.Identity, Vector3.Down * (height * 0.5f))
            });
        }

        return library;
    }

    private ArrayMesh CreateTileMesh(TerrainType terrain, int elevation)
    {
        var height = elevation * TacticalMap.ElevationStep - BaseY;
        var mesh = new ArrayMesh();

        AddSideSurface(mesh, height);
        mesh.SurfaceSetMaterial(mesh.GetSurfaceCount() - 1, _sideMaterials[terrain]);

        AddTopSurface(mesh);
        mesh.SurfaceSetMaterial(mesh.GetSurfaceCount() - 1, _topMaterials[terrain]);

        return mesh;
    }

    private static void AddSideSurface(ArrayMesh mesh, float height)
    {
        var half = TileWidth * 0.5f;
        var bottom = -height;
        var surface = new SurfaceTool();
        surface.Begin(Mesh.PrimitiveType.Triangles);

        AddQuad(surface,
            new Vector3(-half, bottom, -half), new Vector3(half, bottom, -half),
            new Vector3(half, 0, -half), new Vector3(-half, 0, -half));
        AddQuad(surface,
            new Vector3(half, bottom, -half), new Vector3(half, bottom, half),
            new Vector3(half, 0, half), new Vector3(half, 0, -half));
        AddQuad(surface,
            new Vector3(half, bottom, half), new Vector3(-half, bottom, half),
            new Vector3(-half, 0, half), new Vector3(half, 0, half));
        AddQuad(surface,
            new Vector3(-half, bottom, half), new Vector3(-half, bottom, -half),
            new Vector3(-half, 0, -half), new Vector3(-half, 0, half));

        surface.Commit(mesh);
    }

    private static void AddTopSurface(ArrayMesh mesh)
    {
        var half = TopWidth * 0.5f;
        var surface = new SurfaceTool();
        surface.Begin(Mesh.PrimitiveType.Triangles);
        // Godot treats clockwise vertices as front-facing. Keep the top's front
        // face pointing upward so it remains visible with back-face culling.
        AddQuad(surface,
            new Vector3(-half, 0.003f, -half), new Vector3(half, 0.003f, -half),
            new Vector3(half, 0.003f, half), new Vector3(-half, 0.003f, half));
        surface.Commit(mesh);
    }

    private static void AddQuad(
        SurfaceTool surface,
        Vector3 bottomLeft,
        Vector3 bottomRight,
        Vector3 topRight,
        Vector3 topLeft)
    {
        AddVertex(surface, bottomLeft, new Vector2(0, 1));
        AddVertex(surface, bottomRight, new Vector2(1, 1));
        AddVertex(surface, topRight, new Vector2(1, 0));
        AddVertex(surface, bottomLeft, new Vector2(0, 1));
        AddVertex(surface, topRight, new Vector2(1, 0));
        AddVertex(surface, topLeft, new Vector2(0, 0));
    }

    private static void AddVertex(SurfaceTool surface, Vector3 vertex, Vector2 uv)
    {
        surface.SetUV(uv);
        surface.AddVertex(vertex);
    }

    private static StandardMaterial3D TexturedMaterial(Texture2D texture) => new()
    {
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        AlbedoTexture = texture,
        TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
        // Terrain is a closed shell viewed from the outside. Rendering back faces
        // makes internal faces visible through sub-pixel seams during rotation.
        CullMode = BaseMaterial3D.CullModeEnum.Back,
        Roughness = 1.0f
    };
}
