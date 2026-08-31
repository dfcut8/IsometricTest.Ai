using System.Collections.Generic;
using Godot;
using IsometricTestAI.Rendering;

namespace IsometricTestAI.Units;

public partial class UnitVisual : Node3D
{
    private readonly Dictionary<FacingDirection, Texture2D> _frames = new();
    private Sprite3D _sprite = null!;
    private MeshInstance3D _selectionRing = null!;
    private FacingDirection _worldFacing;

    public void Build(Color bodyColor, FacingDirection worldFacing)
    {
        _worldFacing = worldFacing;
        foreach (FacingDirection direction in System.Enum.GetValues(typeof(FacingDirection)))
            _frames[direction] = PrototypeTextures.CreateUnit(direction, bodyColor);

        _sprite = new Sprite3D
        {
            Name = "DirectionalSprite",
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            Shaded = false,
            DoubleSided = true,
            AlphaCut = SpriteBase3D.AlphaCutMode.Discard,
            PixelSize = 0.045f,
            Position = new Vector3(0, 0.53f, 0)
        };
        AddChild(_sprite);

        var ringMaterial = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color("ffe36e"),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            NoDepthTest = true
        };
        _selectionRing = new MeshInstance3D
        {
            Name = "SelectionMarker",
            Mesh = new CylinderMesh { TopRadius = 0.38f, BottomRadius = 0.38f, Height = 0.018f },
            MaterialOverride = ringMaterial,
            Position = new Vector3(0, 0.035f, 0),
            Visible = false
        };
        AddChild(_selectionRing);
        SetCameraQuadrant(0);
    }

    public void SetSelected(bool selected) => _selectionRing.Visible = selected;

    public void SetCameraQuadrant(int cameraQuadrant)
    {
        var relative = ((int)_worldFacing - cameraQuadrant + 4) % 4;
        _sprite.Texture = _frames[(FacingDirection)relative];
    }
}
