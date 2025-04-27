using Godot;
using System;
using NewGameProject.Api;

public partial class Tile() : Node3D
{
    public ushort Metadata;
    public MeshInstance3D Mesh { get; protected set; }
    public bool ShowAlternate { get; set; }
    public bool IsAlive { get; protected set; } = true;

    public bool IsStoodOn { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);

        ShowAlternate = ((int)Position.X % 2 == 1) ^ ((int)Position.Z % 2 == 1);
        Mesh.SetInstanceShaderParameter("brightness", ShowAlternate ? 0.5 : 1.0);

        Position = Position with
        {
            Y = MathUtil.ExpDecay(Position.Y, IsStoodOn ? 0.0f : 0.1f, IsStoodOn ? 40f : 10f, (float)delta)
        };
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
