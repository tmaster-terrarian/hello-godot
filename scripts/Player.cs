using System;
using System.Threading.Tasks;
using Godot;
using NewGameProject.Api;

namespace NewGameProject.Scripts;

public partial class Player(World world) : Node3D
{
    private PackedScene _player = GD.Load<PackedScene>("res://models/player.glb");

    private Node3D _model;
    private Tween _moveTween;
    private AnimationPlayer _animationPlayer;

    private float _animationTime;
    private float _animationTimeTarget;

    private float _moveInputDelay = 0f;
    private float _moveInputWaitTime = 0.1f;
    private float _moveInputFirstWaitTime = 0.25f;
    private bool _moveInputHasWaitedFirstTime = false;

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

        if (Input.IsActionJustPressed("action_primary"))
        {
            if (world.GetTile(new Vector2I((int)Position.X, (int)Position.Z)) is var tile)
            {
                if (!IsInstanceValid(tile)) return;
                tile.OnPower();
            }
        }

        _moveInputDelay = MathUtil.Approach(_moveInputDelay, 0f, (float)delta);
        var inputDir = Vector3.Zero;
        if (Input.IsActionPressed("move_east"))
            inputDir = Vector3.Right;
        else if (Input.IsActionPressed("move_west"))
            inputDir = Vector3.Left;
        else if (Input.IsActionPressed("move_south"))
            inputDir = Vector3.Back;
        else if (Input.IsActionPressed("move_north"))
            inputDir = Vector3.Forward;

        var moveMade = inputDir.X != 0 || inputDir.Z != 0;
        if (moveMade)
        {
            if (_moveInputDelay <= 0 && (_moveTween is null || (!_moveTween.IsValid() && !_moveTween.IsRunning())))
            {
                MoveDelta(new Vector3(inputDir.X, 0, inputDir.Z));
                LookAt(Position + inputDir, Vector3.Up, true);
            }
        }
        else
        {
            _moveInputHasWaitedFirstTime = false;
            _moveInputDelay = 0f;
        }

        _animationTime = MathUtil.ExpDecay(_animationTime, _animationTimeTarget, 16f, (float)delta);
        _animationPlayer.Seek(_animationTime, true);
    }

    private async Task Move(Vector3 position)
    {
        _moveInputDelay = (_moveInputHasWaitedFirstTime ? _moveInputWaitTime : _moveInputFirstWaitTime) + 0.05f;
        _moveInputHasWaitedFirstTime = true;

        _animationTimeTarget += (float)_animationPlayer.CurrentAnimationLength * 0.25f;

        Vector2I currentPosition = new Vector2I((int)Position.X, (int)Position.Z);
        Vector2I targetPosition = new Vector2I((int)position.X, (int)position.Z);
        if (!world.CanMoveToTile(currentPosition, targetPosition))
            return;

        world.StepOffTile(new Vector2I((int)Position.X, (int)Position.Z));
        world.StepOnTile(targetPosition);
        _moveTween = CreateTween()
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _moveTween
            .TweenProperty(this, "position", position, _moveInputWaitTime);
        await ToSignal(_moveTween, "finished");
        // GD.Print("Tween finished.");
    }

    private async Task MoveDelta(Vector3 delta)
    {
        await Move(Position + delta);
    }
}
