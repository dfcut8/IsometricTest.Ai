using System;
using Godot;
using IsometricTestAI.Units;

namespace IsometricTestAI.Rendering;

public static class PrototypeTextures
{
    private static readonly Color Transparent = new(0, 0, 0, 0);
    private static readonly Color Ink = new("172033");

    public static Texture2D CreateUnit(FacingDirection direction, Color body)
    {
        var image = Image.CreateEmpty(16, 24, false, Image.Format.Rgba8);
        image.Fill(Transparent);

        Rect(image, 6, 2, 4, 2, Ink);
        Rect(image, 5, 4, 6, 5, Ink);
        Rect(image, 6, 3, 4, 5, new Color("e6b87a"));
        Rect(image, 4, 9, 8, 9, Ink);
        Rect(image, 5, 9, 6, 8, body);
        Rect(image, 3, 11, 2, 6, Ink);
        Rect(image, 11, 11, 2, 6, Ink);
        Rect(image, 5, 18, 3, 5, Ink);
        Rect(image, 9, 18, 3, 5, Ink);

        switch (direction)
        {
            case FacingDirection.North:
                Rect(image, 6, 3, 4, 2, new Color("71503a"));
                Rect(image, 7, 11, 2, 4, new Color("8ca0b8"));
                break;
            case FacingDirection.East:
                Rect(image, 9, 5, 2, 2, new Color("f6df68"));
                Rect(image, 10, 11, 2, 3, new Color("8ca0b8"));
                break;
            case FacingDirection.South:
                Rect(image, 6, 5, 1, 1, Ink);
                Rect(image, 9, 5, 1, 1, Ink);
                Rect(image, 6, 10, 4, 2, new Color("f6df68"));
                break;
            case FacingDirection.West:
                Rect(image, 5, 5, 2, 2, new Color("f6df68"));
                Rect(image, 4, 11, 2, 3, new Color("8ca0b8"));
                break;
        }

        return ImageTexture.CreateFromImage(image);
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
