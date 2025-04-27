using Godot;
using System;

public partial class Tile() : Node3D
{
    public MeshInstance3D Mesh { get; protected set; }
    public bool ShowAlternate { get; set; }
    public bool IsAlive { get; protected set; } = true;

    public bool IsStoodOn { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);

        ShowAlternate = ((int)Position.X % 2 == 1) ^ ((int)Position.Z % 2 == 1);
        Mesh.SetInstanceShaderParameter("brightness", ShowAlternate ? 0.5 : 1.0);

        Position = Position with { Y = (float)Mathf.Lerp(Position.Y, IsStoodOn ? 0.0f : 0.1f, delta *
            (IsStoodOn ? 40f : 10f)) };
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
