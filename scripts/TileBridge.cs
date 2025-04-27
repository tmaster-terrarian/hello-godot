using Godot;
using NewGameProject.Api;

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
    public Directions Direction { get => (Directions)(Metadata & 1); set => Metadata = (ushort)value; }

    public override void _Ready()
    {
        base._Ready();

        RotateY((float)GD.RandRange(-0.05, 0.05));
        _block = new Node3D();
        AddChild(_block);

        var model = _tile.Instantiate<Node3D>();
        _block.AddChild(model);
        Mesh = model.GetNode("Cube") as MeshInstance3D;

        _block.Rotation = new Vector3(0f, Direction == Directions.Horizontal ? Mathf.DegToRad(90) : 0f, 0f);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        _block.Rotation = _block.Rotation with
        {
            Y = MathUtil.ExpDecay(_block.Rotation.Y, Direction == Directions.Horizontal ? Mathf.DegToRad(90) : 0f, 18,
                (float)delta)
        };
    }

    public override void Interact()
    {
        Direction = Direction == Directions.Horizontal ? Directions.Vertical : Directions.Horizontal;
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
