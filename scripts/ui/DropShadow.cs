using Godot;

namespace NewGameProject.Scripts.UI;

public partial class DropShadow : TextureRect
{
    private static readonly Shader Shader = GD.Load<Shader>("res://shaders/ui/drop_shadow.gdshader");
    private static readonly Material ShadowMaterial = new ShaderMaterial() { Shader = Shader };
    public required Control TargetNode { get; set; }
    public required Vector2 Offset { get; set; }

    private Color _color = Colors.Black;
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            SetInstanceShaderParameter("shadow_color", value);
        }
    }

    public override void _Ready()
    {
        base._Ready();

        Modulate = Colors.Black;
        ShowBehindParent = true;

        Material = ShadowMaterial;
        SetInstanceShaderParameter("shadow_color", Color);

        UpdatePosition();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        GlobalPosition = TargetNode.GlobalPosition + Offset;
    }
}
