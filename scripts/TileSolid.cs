using System;
using Godot;

namespace NewGameProject.scripts;

public partial class TileSolid : Tile
{
    private PackedScene _tile = GD.Load<PackedScene>("res://models/tile_solid.glb");

    private Node3D _block;
    private double _shakeTime;

    public override void _Ready()
    {
        base._Ready();


        _block = new Node3D();
        _block.RotateY((float)GD.RandRange(-0.05, 0.05));
        AddChild(_block);

        var model = _tile.Instantiate<Node3D>();
        _block.AddChild(model);
        Mesh = model.GetNode("Cube") as MeshInstance3D;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_shakeTime > 0)
        {
            _shakeTime -= delta;
            double range = 0.02;
            _block.Position = new Vector3((float)GD.RandRange(-range, range), 0, (float)GD.RandRange(-range, range));
        }
        else
        {
            _block.Position = new Vector3(0, 0, 0);
        }
    }

    public override void Interact()
    {
        var tween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        float rotY;
        do
        {
            rotY = (float)GD.RandRange(-0.05, 0.05);
        }
        while (Math.Abs(rotY - _block.Rotation.Y) < 0.05);
        tween.TweenProperty(_block, "rotation", new Vector3(0, rotY, 0), 0.1f);
        _shakeTime = 0.2;
    }
}
