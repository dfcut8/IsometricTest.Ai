using System;
using Godot;

namespace IsometricTestAI.Camera;

public partial class TacticalCameraController : Node3D
{
    public event Action<int>? VisualQuadrantChanged;
    public event Action<int>? RotationCompleted;

    public int CameraQuadrant { get; private set; }
    public Camera3D Camera { get; private set; } = null!;

    private Tween? _rotationTween;
    private float _targetYawDegrees = 45.0f;
    private int _visualQuadrant = -1;

    public void Build()
    {
        Name = "CameraPivot";
        Position = new Vector3(4.5f, 0.25f, 4.5f);
        RotationDegrees = new Vector3(0, _targetYawDegrees, 0);

        Camera = new Camera3D
        {
            Name = "Camera3D",
            Projection = Camera3D.ProjectionType.Orthogonal,
            Size = 14.2f,
            Position = new Vector3(0, 10.5f, 14.5f),
            RotationDegrees = new Vector3(-36, 0, 0),
            Current = true,
            Near = 0.1f,
            Far = 100.0f
        };
        AddChild(Camera);
        UpdateVisualQuadrant();
    }

    public override void _Process(double delta) => UpdateVisualQuadrant();

    public void RotateLeft() => RotateBy(-1);
    public void RotateRight() => RotateBy(1);

    private void RotateBy(int direction)
    {
        if (_rotationTween is { } tween && tween.IsRunning())
            return;

        CameraQuadrant = (CameraQuadrant + direction + 4) % 4;
        _targetYawDegrees += direction * 90.0f;
        _rotationTween = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
        _rotationTween.TweenProperty(this, "rotation:y", Mathf.DegToRad(_targetYawDegrees), 0.3);
        _rotationTween.Finished += () =>
        {
            RotationDegrees = new Vector3(0, _targetYawDegrees, 0);
            UpdateVisualQuadrant();
            RotationCompleted?.Invoke(CameraQuadrant);
        };
    }

    private void UpdateVisualQuadrant()
    {
        var relativeDegrees = Mathf.RadToDeg(Rotation.Y) - 45.0f;
        var nearest = Mathf.RoundToInt(relativeDegrees / 90.0f);
        nearest = (nearest % 4 + 4) % 4;
        if (nearest == _visualQuadrant)
            return;
        _visualQuadrant = nearest;
        VisualQuadrantChanged?.Invoke(_visualQuadrant);
    }
}
