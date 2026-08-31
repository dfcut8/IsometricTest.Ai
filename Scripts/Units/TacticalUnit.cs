using Godot;
using IsometricTestAI.Map;

namespace IsometricTestAI.Units;

public partial class TacticalUnit : Area3D
{
    public UnitState State { get; private set; } = null!;
    private TacticalMap _map = null!;
    private UnitVisual _visual = null!;
    private Tween? _moveTween;

    public void Build(UnitState state, TacticalMap map, Color bodyColor)
    {
        State = state;
        _map = map;
        Name = state.Name;
        CollisionLayer = 2;
        CollisionMask = 0;
        Position = map.GridToWorld(state.GridPosition);

        _visual = new UnitVisual { Name = "Visual" };
        AddChild(_visual);
        _visual.Build(bodyColor, state.Facing);

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

    public void AnimateToLogicalPosition()
    {
        _moveTween?.Kill();
        _moveTween = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quad);
        _moveTween.TweenProperty(this, "position", _map.GridToWorld(State.GridPosition), 0.22);
    }
}
