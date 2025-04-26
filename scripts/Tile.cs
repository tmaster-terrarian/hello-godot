using Godot;
using System;

public partial class Tile() : Node3D
{
    public MeshInstance3D Mesh { get; protected set; }
    public bool ShowAlternate { get; set; }
    public bool IsAlive { get; protected set; } = true;

    public override void _Process(double delta)
    {
        base._Process(delta);

        Mesh.SetInstanceShaderParameter("brightness", ShowAlternate ? 0.5 : 1.0);
    }

    public virtual void Interact() { }
}
