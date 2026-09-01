using System;
using System.Collections.Generic;
using Godot;
using IsometricTestAI.Map;

namespace IsometricTestAI.Units;

public partial class TacticalUnit : Area3D
{
    public event Action? MovementCompleted;

    public UnitState State { get; private set; } = null!;
    public bool IsMoving { get; private set; }
    private TacticalMap _map = null!;
    private UnitVisual _visual = null!;
    private Tween? _moveTween;

    public void Build(UnitState state, TacticalMap map, UnitAppearance appearance)
    {
        State = state;
        _map = map;
        Name = state.Name;
        CollisionLayer = 2;
        CollisionMask = 0;
        Position = map.GridToWorld(state.GridPosition);

        _visual = new UnitVisual { Name = "Visual" };
        AddChild(_visual);
        _visual.Build(appearance, state.Facing);

        var collider = new CollisionShape3D
        {
            Name = "PickCollider",
            Shape = new BoxShape3D { Size = new Vector3(0.55f, 1.15f, 0.55f) },
            Position = new Vector3(0, 0.55f, 0)
        };
        AddChild(collider);
    }

    public void SetCameraQuadrant(int quadrant) => _visual.SetCameraQuadrant(quadrant);
    public void SetSelected(bool selected) => _visual.SetSelected(selected);

    public bool MoveAlongPath(IReadOnlyList<Vector2I> path)
    {
        if (IsMoving || path.Count < 2 || path[0] != State.GridPosition)
            return false;

        _moveTween?.Kill();
        IsMoving = true;
        _moveTween = CreateTween();

        for (var index = 1; index < path.Count; index++)
        {
            var next = path[index];
            _moveTween.TweenCallback(Callable.From(() => BeginStep(next)));
            _moveTween.TweenProperty(this, "position", _map.GridToWorld(next), 0.18)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Quad);
        }

        _moveTween.TweenCallback(Callable.From(FinishMovement));
        return true;
    }

    private void BeginStep(Vector2I next)
    {
        var delta = next - State.GridPosition;
        if (!_map.TryMoveUnit(State, next))
        {
            _moveTween?.Kill();
            FinishMovement();
            return;
        }

        State.Facing = FacingFromDelta(delta);
        _visual.SetWorldFacing(State.Facing);
    }

    private void FinishMovement()
    {
        if (!IsMoving)
            return;
        IsMoving = false;
        _moveTween = null;
        MovementCompleted?.Invoke();
    }

    private static FacingDirection FacingFromDelta(Vector2I delta)
    {
        if (delta.X > 0)
            return FacingDirection.East;
        if (delta.X < 0)
            return FacingDirection.West;
        return delta.Y > 0 ? FacingDirection.South : FacingDirection.North;
    }
}
