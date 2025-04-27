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

    public bool IsDeleting { get; private set; }

    protected void FinishInitialization()
    {
        ShowAlternate = ((int)Position.X % 2 == 1) ^ ((int)Position.Z % 2 == 1);
        Mesh.SetInstanceShaderParameter("brightness", ShowAlternate ? 0.5 : 1.0);

        float randomRange = 0.1f;
        float randomRangeSmall = 0.025f;
        RotateY((float)GD.RandRange(-randomRange, randomRange));
        RotateX((float)GD.RandRange(-randomRangeSmall, randomRangeSmall));
        RotateZ((float)GD.RandRange(-randomRangeSmall, randomRangeSmall));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Position = Position with
        {
            Y = MathUtil.ExpDecay(Position.Y, IsStoodOn ? -0.1f : 0.0f, IsStoodOn ? 40f : 10f, (float)delta)
        };
    }

    public virtual void OnStepOn() { }
    public virtual void OnStepOff() { }

    public virtual bool CanStepOff(Vector2I direction)
    {
        return true;
    }
    public virtual bool CanStepOn(Vector2I direction)
    {
        return true;
    }

    public void EditorCreate()
    {
        Scale = Vector3.Zero;

        var tween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
        tween.TweenProperty(this, "scale", Vector3.One, 0.2f);
    }

    public void EditorDelete()
    {
        IsDeleting = true;
        var tween = CreateTween()
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Linear);
        tween.TweenProperty(this, "scale", Vector3.Zero, 0.1f).Finished += this.QueueFree;
    }
}
