using System;
using System.Threading.Tasks;
using Godot;

namespace NewGameProject.Scripts;

public partial class Player(Action<Vector2I> onStepOffTile, Func<Vector2I, Vector2I, bool> canMoveToTile) : Node3D
{
    private PackedScene _player = GD.Load<PackedScene>("res://models/player.glb");

    private Node3D _model;
    private Tween _moveTween;
    private AnimationPlayer _animationPlayer;

    private float _animationTime;
    private float _animationTimeTarget;

    public override void _Ready()
    {
        base._Ready();

        _model = _player.Instantiate<Node3D>();
        _animationPlayer = _model.GetNode<AnimationPlayer>("AnimationPlayer");
        _animationPlayer.Play("walk");
        _animationPlayer.Advance(0);
        _animationPlayer.Pause();
        _model.GetNode<Node3D>("RoboBoy/Skeleton3D/Ice Head").SetVisible(false);
        _model.GetNode<Node3D>("RoboBoy/Skeleton3D/Water Head").SetVisible(false);
        _model.GetNode<Node3D>("Pick").SetVisible(false);
        AddChild(_model);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var inputDir = Vector3.Zero;
        if (Input.IsActionJustPressed("move_east"))
            inputDir = Vector3.Right;
        else if (Input.IsActionJustPressed("move_west"))
            inputDir = Vector3.Left;
        else if (Input.IsActionJustPressed("move_south"))
            inputDir = Vector3.Back;
        else if (Input.IsActionJustPressed("move_north"))
            inputDir = Vector3.Forward;

        var moveMade = inputDir.X != 0 || inputDir.Z != 0;
        if (moveMade && (_moveTween is null || (!_moveTween.IsValid() && !_moveTween.IsRunning())))
        {
            MoveDelta(new Vector3(inputDir.X, 0, inputDir.Z));
            LookAt(Position + inputDir, Vector3.Up, true);
        }

        _animationTime = Mathf.Lerp(_animationTime, _animationTimeTarget, (float)delta * 20f);
        _animationPlayer.Seek(_animationTime, true);
    }

    private async Task Move(Vector3 position)
    {
        _animationTimeTarget += (float)_animationPlayer.CurrentAnimationLength * 0.25f;

        Vector2I currentPosition = new Vector2I((int)Position.X, (int)Position.Z);
        Vector2I targetPosition = new Vector2I((int)position.X, (int)position.Z);
        if (!canMoveToTile.Invoke(currentPosition, targetPosition))
            return;

        onStepOffTile?.Invoke(new Vector2I((int)Position.X, (int)Position.Z));
        _moveTween = CreateTween()
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _moveTween
            .TweenProperty(this, "position", position, 0.1f);
        await ToSignal(_moveTween, "finished");
        // GD.Print("Tween finished.");
    }

    private async Task MoveDelta(Vector3 delta)
    {
        await Move(Position + delta);
    }
}
