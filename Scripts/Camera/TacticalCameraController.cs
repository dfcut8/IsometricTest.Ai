using Godot;
using System;

namespace IsometricTestAI.Camera;

public partial class TacticalCameraController : Node3D
{
    private const float DefaultCameraSize = 4.2f;
    private const float MinimumCameraSize = 2.4f;
    private const float MaximumCameraSize = 8.0f;
    private const float ZoomFactor = 1.15f;
    private const float ZoomSmoothing = 14.0f;

    public event Action<int>? VisualQuadrantChanged;
    public event Action<int>? RotationCompleted;

    public int CameraQuadrant { get; private set; }
    public Camera3D Camera { get; private set; } = null!;

    private Tween? _rotationTween;
    private float _targetYawDegrees = 45.0f;
    private float _targetCameraSize = DefaultCameraSize;
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
            Size = DefaultCameraSize,
            Position = new Vector3(0, 10.5f, 14.5f),
            // A 30-degree pitch with the pivot's 45-degree yaw projects square
            // ground tiles as exact 2:1 isometric diamonds.
            RotationDegrees = new Vector3(-30, 0, 0),
            Current = true,
            Near = 0.1f,
            Far = 100.0f
        };
        AddChild(Camera);
        UpdateVisualQuadrant();
    }

    public override void _Process(double delta)
    {
        UpdateVisualQuadrant();

        if (Mathf.IsEqualApprox(Camera.Size, _targetCameraSize))
            return;

        var blend = 1.0f - Mathf.Exp(-ZoomSmoothing * (float)delta);
        Camera.Size = Mathf.Lerp(Camera.Size, _targetCameraSize, blend);
        if (Mathf.Abs(Camera.Size - _targetCameraSize) < 0.001f)
            Camera.Size = _targetCameraSize;
    }

    public void RotateLeft() => RotateBy(-1);
    public void RotateRight() => RotateBy(1);
    public void ZoomIn(float steps = 1.0f) => ChangeZoom(-steps);
    public void ZoomOut(float steps = 1.0f) => ChangeZoom(steps);

    private void ChangeZoom(float steps)
    {
        var zoomMultiplier = Mathf.Pow(ZoomFactor, steps);
        _targetCameraSize = Mathf.Clamp(
            _targetCameraSize * zoomMultiplier,
            MinimumCameraSize,
            MaximumCameraSize);
    }

    private void RotateBy(int direction)
    {
        if (_rotationTween is { } tween && tween.IsRunning())
            return;

        CameraQuadrant = (CameraQuadrant + direction + 4) % 4;
        _targetYawDegrees += direction * 90.0f;
        _rotationTween = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
        _rotationTween.TweenProperty(this, "rotation:y", Mathf.DegToRad(_targetYawDegrees), 1.3);
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
