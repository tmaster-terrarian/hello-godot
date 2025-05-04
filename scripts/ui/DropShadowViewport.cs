using Godot;

namespace NewGameProject.Scripts.UI;

public partial class DropShadowViewport : DropShadow
{
    public required Viewport Viewport;

    public override void _Ready()
    {
        base._Ready();

        Texture = Viewport.GetTexture();
    }
}
