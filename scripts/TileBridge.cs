using Godot;

namespace NewGameProject.Scripts;

public partial class TileBridge : Tile
{
    private PackedScene _tile = GD.Load<PackedScene>("res://models/tile_bridge.glb");

    private Node3D _block;

    public enum Directions
    {
        Vertical,
        Horizontal,
    }
    public Directions Direction = Directions.Vertical;

    public override void _Ready()
    {
        base._Ready();

        RotateY((float)GD.RandRange(-0.05, 0.05));
        _block = new Node3D();
        AddChild(_block);

        var model = _tile.Instantiate<Node3D>();
        _block.AddChild(model);
        Mesh = model.GetNode("Cube") as MeshInstance3D;
    }

    public override void Interact()
    {
        Direction = Direction == Directions.Horizontal ? Directions.Vertical : Directions.Horizontal;
        var tween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(
            _block,
"rotation",
            new Vector3(0, Direction == Directions.Horizontal ? Mathf.DegToRad(90) : 0, 0),
    0.2f
        );
    }

    public override bool CanStepOff(Vector2I direction)
    {
        return CanStepOn(direction);
    }
    public override bool CanStepOn(Vector2I direction)
    {
        return direction.X != 0 && Direction == Directions.Horizontal ||
               direction.Y != 0 && Direction == Directions.Vertical;
    }
}
