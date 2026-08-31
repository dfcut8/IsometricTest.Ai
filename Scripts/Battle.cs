using System.Collections.Generic;
using Godot;
using IsometricTestAI.Camera;
using IsometricTestAI.Input;
using IsometricTestAI.Map;
using IsometricTestAI.Rendering;
using IsometricTestAI.Units;

namespace IsometricTestAI;

public partial class Battle : Node3D
{
    private TacticalMap _map = null!;
    private TacticalCameraController _cameraController = null!;
    private readonly List<TacticalUnit> _units = new();
    private MeshInstance3D _hoverMarker = null!;
    private MeshInstance3D _selectedMarker = null!;
    private Label _cameraLabel = null!;
    private Label _hoverLabel = null!;
    private Label _selectionLabel = null!;

    public override void _Ready()
    {
        _map = new TacticalMap();
        BuildEnvironment();

        var world = new Node3D { Name = "World" };
        AddChild(world);

        var terrain = new TerrainRenderer();
        world.AddChild(terrain);
        terrain.Build(_map);

        var objects = new Node3D { Name = "Objects" };
        var unitsNode = new Node3D { Name = "Units" };
        world.AddChild(objects);
        world.AddChild(unitsNode);
        BuildObjects(objects);
        BuildUnits(unitsNode);
        BuildMarkers(world);

        _cameraController = new TacticalCameraController();
        AddChild(_cameraController);
        _cameraController.Build();
        _cameraController.VisualQuadrantChanged += UpdateDirectionalSprites;
        _cameraController.RotationCompleted += UpdateCameraLabel;
        UpdateDirectionalSprites(0);

        BuildUi();

        var input = new BattleInputController { Name = "BattleInputController" };
        AddChild(input);
        input.Configure(_map, _cameraController);
        input.HoveredCellChanged += UpdateHover;
        input.SelectedCellChanged += UpdateSelectedCell;
        input.SelectedUnitChanged += UpdateSelectedUnit;
    }

    private void BuildUnits(Node3D parent)
    {
        AddUnit(parent, new UnitState("Amber", new Vector2I(3, 6), FacingDirection.North), UnitAppearance.Fighter);
        AddUnit(parent, new UnitState("Teal", new Vector2I(6, 3), FacingDirection.East), UnitAppearance.Scout);
        AddUnit(parent, new UnitState("Violet", new Vector2I(7, 7), FacingDirection.West), UnitAppearance.Mage);
    }

    private void AddUnit(Node3D parent, UnitState state, UnitAppearance appearance)
    {
        _map.AddUnit(state);
        var unit = new TacticalUnit();
        parent.AddChild(unit);
        unit.Build(state, _map, appearance);
        _units.Add(unit);
    }

    private void BuildObjects(Node3D parent)
    {
        AddObject(parent, "TreeA", new Vector2I(1, 2), PrototypeTextures.CreateTree(), 0.042f, 0.60f);
        AddObject(parent, "TreeB", new Vector2I(8, 3), PrototypeTextures.CreateTree(), 0.042f, 0.60f);
        AddObject(parent, "CrateA", new Vector2I(4, 4), PrototypeTextures.CreateCrate(), 0.040f, 0.36f);
        AddObject(parent, "CrateB", new Vector2I(6, 8), PrototypeTextures.CreateCrate(), 0.040f, 0.36f);
    }

    private void AddObject(Node3D parent, string name, Vector2I cell, Texture2D texture, float pixelSize, float height)
    {
        parent.AddChild(new Sprite3D
        {
            Name = name,
            Texture = texture,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            Shaded = false,
            DoubleSided = true,
            AlphaCut = SpriteBase3D.AlphaCutMode.Discard,
            PixelSize = pixelSize,
            Position = _map.GridToWorld(cell) + Vector3.Up * height
        });
    }

    private void BuildMarkers(Node3D parent)
    {
        _hoverMarker = CreateMarker("HoverCell", new Color(1.0f, 1.0f, 0.55f, 0.36f), 0.91f);
        _selectedMarker = CreateMarker("SelectedCell", new Color(0.20f, 0.72f, 1.0f, 0.48f), 0.72f);
        parent.AddChild(_hoverMarker);
        parent.AddChild(_selectedMarker);
    }

    private static MeshInstance3D CreateMarker(string name, Color color, float width)
    {
        return new MeshInstance3D
        {
            Name = name,
            Mesh = new BoxMesh { Size = new Vector3(width, 0.025f, width) },
            MaterialOverride = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                AlbedoColor = color,
                NoDepthTest = true
            },
            Visible = false
        };
    }

    private void BuildUi()
    {
        var ui = new CanvasLayer { Name = "UI" };
        AddChild(ui);
        var panel = new ColorRect
        {
            Color = new Color(0.035f, 0.05f, 0.08f, 0.78f),
            Position = new Vector2(6, 6),
            Size = new Vector2(126, 72),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        ui.AddChild(panel);
        var box = new VBoxContainer
        {
            Position = new Vector2(10, 9),
            Size = new Vector2(185, 100),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        ui.AddChild(box);
        box.AddChild(MakeLabel("Q / E - Rotate Camera", new Color("ffe36e")));
        _cameraLabel = MakeLabel("Camera: 0°", Colors.White);
        _hoverLabel = MakeLabel("Hovered Cell: --", Colors.White);
        _selectionLabel = MakeLabel("Selected Unit: none", Colors.White);
        box.AddChild(_cameraLabel);
        box.AddChild(_hoverLabel);
        box.AddChild(_selectionLabel);
    }

    private static Label MakeLabel(string text, Color color)
    {
        var label = new Label { Text = text, MouseFilter = Control.MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", 8);
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeColorOverride("font_outline_color", new Color("101522"));
        label.AddThemeConstantOverride("outline_size", 2);
        return label;
    }

    private void BuildEnvironment()
    {
        var environment = new Environment
        {
            BackgroundMode = Environment.BGMode.Color,
            BackgroundColor = new Color("1b2733"),
            AmbientLightSource = Environment.AmbientSource.Color,
            AmbientLightColor = Colors.White,
            AmbientLightEnergy = 0.8f
        };
        AddChild(new WorldEnvironment { Name = "WorldEnvironment", Environment = environment });
    }

    private void UpdateDirectionalSprites(int quadrant)
    {
        foreach (var unit in _units)
            unit.SetCameraQuadrant(quadrant);
    }

    private void UpdateCameraLabel(int quadrant) => _cameraLabel.Text = $"Camera: {quadrant * 90}°";

    private void UpdateHover(Vector2I? cell)
    {
        _hoverMarker.Visible = cell.HasValue;
        _hoverLabel.Text = cell is { } value ? $"Hovered Cell: {value.X},{value.Y}" : "Hovered Cell: --";
        if (cell is { } position)
            PositionMarker(_hoverMarker, position, 0.045f);
    }

    private void UpdateSelectedCell(Vector2I? cell)
    {
        _selectedMarker.Visible = cell.HasValue;
        if (cell is { } position)
            PositionMarker(_selectedMarker, position, 0.065f);
    }

    private void UpdateSelectedUnit(TacticalUnit? unit)
    {
        _selectionLabel.Text = unit == null
            ? "Selected Unit: none"
            : $"Selected Unit: {unit.State.Name}\nPosition: {unit.State.GridPosition.X},{unit.State.GridPosition.Y}  Facing: {unit.State.Facing}";
    }

    private void PositionMarker(Node3D marker, Vector2I cell, float offset)
    {
        marker.Position = _map.GridToWorld(cell) + Vector3.Up * offset;
    }
}
