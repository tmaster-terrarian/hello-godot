using Godot;

namespace NewGameProject.Scripts;

public partial class TileFall : Tile
{
    private PackedScene _tile = GD.Load<PackedScene>("res://models/tile.glb");

    private MeshInstance3D _sphere;

    private Node3D _block;
    private float _time;

    public override void _Ready()
    {
        base._Ready();

        _time = (float)GD.RandRange(0.0, 300.0);

        _block = new Node3D();
        AddChild(_block);

        var model = _tile.Instantiate<Node3D>();
        _block.AddChild(model);
        Mesh = model.GetNode("Cube") as MeshInstance3D;
        _sphere = model.GetNode("Sphere") as MeshInstance3D;

        FinishInitialization();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!IsAlive) return;
        _time += (float)delta;
        _block.Rotation = new Vector3(0, Mathf.Sin(_time * 12) * 0.05f, 0);
        _sphere.SetInstanceShaderParameter("brightness", ShowAlternate ? 0.5 : 1.0);
    }

    public override void OnStepOff()
    {
        ActionFall();
    }

    public override void OnPower()
    {
        if (IsAlive) ActionFall();
        else ActionRise();
    }

    public override bool CanStepOn(Vector2I direction)
    {
        return IsAlive;
    }

    private void ActionFall()
    {
        IsAlive = false;
        var tween = CreateTween()
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(Mesh, "scale", Vector3.Zero, 0.2f);

        var tween2 = CreateTween()
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Back);
        tween2.TweenProperty(Mesh, "rotation", new Vector3(0, Mathf.DegToRad(90), 0), 0.2f);
    }

    private void ActionRise()
    {
        IsAlive = true;
        var tween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(Mesh, "scale", Vector3.One, 0.2f);

        var tween2 = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
        tween2.TweenProperty(Mesh, "rotation", new Vector3(0, Mathf.DegToRad(0), 0), 0.2f);
    }
}
