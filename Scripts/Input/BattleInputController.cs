using System;
using Godot;
using IsometricTestAI.Camera;
using IsometricTestAI.Map;
using IsometricTestAI.Units;

namespace IsometricTestAI.Input;

public partial class BattleInputController : Node
{
    public event Action<Vector2I?>? HoveredCellChanged;
    public event Action<Vector2I?>? SelectedCellChanged;
    public event Action<TacticalUnit?>? SelectedUnitChanged;

    private TacticalMap _map = null!;
    private TacticalCameraController _cameraController = null!;
    private Vector2I? _hovered;
    private TacticalUnit? _selectedUnit;
    private Vector2I? _selectedCell;

    public void Configure(TacticalMap map, TacticalCameraController cameraController)
    {
        _map = map;
        _cameraController = cameraController;
    }

    public override void _Process(double delta)
    {
        var hit = RaycastAtMouse();
        var cell = CellFromHit(hit);
        if (cell == _hovered)
            return;
        _hovered = cell;
        HoveredCellChanged?.Invoke(_hovered);
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey key && key.Pressed && !key.Echo)
        {
            if (key.Keycode == Key.Q)
                _cameraController.RotateLeft();
            else if (key.Keycode == Key.E)
                _cameraController.RotateRight();
        }

        if (inputEvent is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
            return;

        var hit = RaycastAtMouse();
        if (hit.Count == 0)
            return;

        var collider = hit["collider"].AsGodotObject();
        if (collider is TacticalUnit unit)
        {
            SelectUnit(unit);
            SelectCell(unit.State.GridPosition);
            return;
        }

        var target = CellFromHit(hit);
        if (target is not { } cell)
            return;

        SelectCell(cell);
        if (_selectedUnit != null && _map.TryMoveUnit(_selectedUnit.State, cell))
        {
            _selectedUnit.AnimateToLogicalPosition();
            SelectedUnitChanged?.Invoke(_selectedUnit);
        }
    }

    private Godot.Collections.Dictionary RaycastAtMouse()
    {
        var camera = _cameraController.Camera;
        var mouse = GetViewport().GetMousePosition();
        var origin = camera.ProjectRayOrigin(mouse);
        var end = origin + camera.ProjectRayNormal(mouse) * 100.0f;
        var query = PhysicsRayQueryParameters3D.Create(origin, end, 3);
        query.CollideWithAreas = true;
        query.CollideWithBodies = true;
        return camera.GetWorld3D().DirectSpaceState.IntersectRay(query);
    }

    private static Vector2I? CellFromHit(Godot.Collections.Dictionary hit)
    {
        if (hit.Count == 0)
            return null;
        var collider = hit["collider"].AsGodotObject();
        if (collider is TacticalUnit unit)
            return unit.State.GridPosition;
        if (collider is Node node && node.HasMeta("grid_position"))
            return node.GetMeta("grid_position").AsVector2I();
        return null;
    }

    private void SelectUnit(TacticalUnit unit)
    {
        if (_selectedUnit == unit)
            return;
        _selectedUnit?.SetSelected(false);
        _selectedUnit = unit;
        _selectedUnit.SetSelected(true);
        SelectedUnitChanged?.Invoke(_selectedUnit);
    }

    private void SelectCell(Vector2I cell)
    {
        _selectedCell = cell;
        SelectedCellChanged?.Invoke(_selectedCell);
    }
}
