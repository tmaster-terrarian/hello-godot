using Godot;
using System;

namespace NewGameProject.Scripts.UI;
public partial class UiTileRenderer : SubViewport
{
    [Export]
    public Node3D RootNode { get; set; }

    public required Node3D Contents { get; set; }

    public override void _Ready()
    {
        base._Ready();

        RootNode.Position = Vector3.Down * 0.25f;
        RootNode.AddChild(Contents);
    }
}
