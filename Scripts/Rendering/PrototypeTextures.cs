using System;
using Godot;
using IsometricTestAI.Map;
using IsometricTestAI.Units;

namespace IsometricTestAI.Rendering;

public static class PrototypeTextures
{
    private static readonly Color Transparent = new(0, 0, 0, 0);
    private static readonly Color Ink = new("172033");

    public static Texture2D LoadUnit(UnitAppearance appearance, FacingDirection direction)
    {
        var unitName = appearance switch
        {
            UnitAppearance.Fighter => "fighter",
            UnitAppearance.Scout => "scout",
            UnitAppearance.Mage => "mage",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
        var facingName = direction.ToString().ToLowerInvariant();
        return GD.Load<Texture2D>($"res://Assets/PixelArt/Units/{unitName}_{facingName}.png");
    }

    public static Texture2D LoadTerrainTop(TerrainType terrain)
    {
        return LoadTerrainTexture(terrain, "top");
    }

    public static Texture2D LoadTerrainSide(TerrainType terrain)
    {
        return LoadTerrainTexture(terrain, "side");
    }

    private static Texture2D LoadTerrainTexture(TerrainType terrain, string face)
    {
        var terrainName = terrain switch
        {
            TerrainType.Grass => "grass",
            TerrainType.Dirt => "dirt",
            TerrainType.Rock => "rock",
            _ => throw new ArgumentOutOfRangeException(nameof(terrain), terrain, null)
        };
        return GD.Load<Texture2D>($"res://Assets/PixelArt/Terrain/{terrainName}_{face}.png");
    }

    public static Texture2D CreateTree()
    {
        var image = Image.CreateEmpty(20, 28, false, Image.Format.Rgba8);
        image.Fill(Transparent);
        Rect(image, 8, 16, 5, 11, new Color("513829"));
        Rect(image, 3, 7, 15, 13, Ink);
        Rect(image, 4, 5, 12, 14, new Color("24584a"));
        Rect(image, 7, 2, 7, 12, new Color("3b8063"));
        Rect(image, 5, 8, 4, 4, new Color("75aa62"));
        return ImageTexture.CreateFromImage(image);
    }

    public static Texture2D CreateCrate()
    {
        var image = Image.CreateEmpty(18, 18, false, Image.Format.Rgba8);
        image.Fill(Transparent);
        Rect(image, 2, 2, 14, 15, Ink);
        Rect(image, 3, 3, 12, 13, new Color("a9683a"));
        Rect(image, 4, 4, 2, 11, new Color("d89b54"));
        Rect(image, 12, 4, 2, 11, new Color("74452f"));
        for (var i = 0; i < 8; i++)
            image.SetPixel(5 + i, 5 + i, Ink);
        return ImageTexture.CreateFromImage(image);
    }

    private static void Rect(Image image, int x, int y, int width, int height, Color color)
    {
        for (var px = Math.Max(0, x); px < Math.Min(image.GetWidth(), x + width); px++)
        for (var py = Math.Max(0, y); py < Math.Min(image.GetHeight(), y + height); py++)
            image.SetPixel(px, py, color);
    }
}
