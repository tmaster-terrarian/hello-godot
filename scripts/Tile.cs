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

        ShowAlternate = ((int)Position.X % 2 == 1) ^ ((int)Position.Z % 2 == 1);
        Mesh.SetInstanceShaderParameter("brightness", ShowAlternate ? 0.5 : 1.0);
    }

    public virtual void Interact() { }

    public virtual bool CanStepOff(Vector2I direction)
    {
        return true;
    }
    public virtual bool CanStepOn(Vector2I direction)
    {
        return true;
    }
}
