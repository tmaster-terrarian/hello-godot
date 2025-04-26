using Godot;

namespace NewGameProject.scripts;

public partial class TileSolid : Tile
{
    private PackedScene _tile = GD.Load<PackedScene>("res://models/tile_solid.glb");

    private Node3D _block;

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
}
