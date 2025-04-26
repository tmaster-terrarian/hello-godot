using Godot;

namespace NewGameProject.scripts;

public partial class TileFall : Tile
{
    private PackedScene _tile = GD.Load<PackedScene>("res://models/tile.glb");

    private Node3D _block;
    private float _time;

    public override void _Ready()
    {
        base._Ready();

        _time = (float)GD.RandRange(0.0, 300.0);

        RotateY((float)GD.RandRange(-0.1, 0.1));

        _block = new Node3D();
        AddChild(_block);

        var model = _tile.Instantiate<Node3D>();
        _block.AddChild(model);
        Mesh = model.GetNode("Cube") as MeshInstance3D;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        _time += (float)delta;
        _block.Rotation = new Vector3(0, Mathf.Sin(_time * 12) * 0.05f, 0);
    }

    public override void Interact()
    {
        var tween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(_block, "scale", new Vector3(0, 0, 0), 0.2f);

        IsAlive = false;
    }

    public override bool CanStepOn(Vector2I direction)
    {
        return IsAlive;
    }
}
