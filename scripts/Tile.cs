using Godot;
using System;

public partial class Tile : Node3D
{
    [Export]
    public Node3D Block { get; set; }

    [Export]
    public Flag Flag { get; set; }

    public bool IsAlive { get; private set; } = true;
    public event Action FlagActiveChanged;

    private bool _flagActive = false;

    private float _time;

    [Export]
    public bool FlagActive
    {
        get => _flagActive;
        set
        {
            if (_flagActive == value)
                return;

            _flagActive = value;
            Flag.AnimationPlayer.CurrentAnimation = _flagActive
                ? Flag.AnimAppear : Flag.AnimRemove;

            FlagActiveChanged?.Invoke();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        _time = (float)GD.RandRange(0.0, 300.0);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        _time += (float)delta;
        Block.Rotation = new Vector3(0, Mathf.Sin(_time * 12) * 0.05f, 0);
    }

    public void Interact()
    {
        var tween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(Block, "scale", new Vector3(0, 0, 0), 0.2f);

        IsAlive = false;
    }
}
