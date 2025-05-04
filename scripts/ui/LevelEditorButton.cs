using Godot;
using NewGameProject.Api;

namespace NewGameProject.Scripts.UI;

public partial class LevelEditorButton : Container
{
    private bool _isHovered = false;
    private float _hoverTime = 0;

    public override void _Ready()
    {
        base._Ready();

        MouseEntered += () => { _isHovered = true; };
        MouseExited += () => { _isHovered = false; };

        SetPivotOffset(Size * 0.5f);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Scale = MathUtil.ExpDecay(Scale, _isHovered ? Vector2.One * 1.5f : Vector2.One, 16f, (float)delta);

        if (_isHovered)
        {
            _hoverTime += (float)delta;
            RotationDegrees = MathUtil.ExpDecay(RotationDegrees, Mathf.Cos(_hoverTime * 10f) * 15f, 16f, (float)delta);
        }
        else
        {
            _hoverTime = 0;
            RotationDegrees = 0;
        }
    }
}
