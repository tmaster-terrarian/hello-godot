using Godot;
using System;

namespace NewGameProject.Scripts.UI;
public partial class UiLevelEditor : PanelContainer
{
    private PackedScene _uiTileRendererScene = GD.Load<PackedScene>("res://scenes/ui/ui_tile_renderer.tscn");

    [Export]
    public Container TilesListNode { get; set; }

    public Node3D[] TileNodes { get; set; }

    public override void _Ready()
    {
        base._Ready();

        foreach (var tileNode in TileNodes)
        {
            SubViewportContainer subViewportContainer = new SubViewportContainer()
            {
                Stretch = true,
                CustomMinimumSize = new Vector2(200, 200),
            };
            TilesListNode.AddChild(subViewportContainer);

            var tileRenderer = _uiTileRendererScene.Instantiate<UiTileRenderer>();
            tileRenderer.Contents = tileNode;
            subViewportContainer.AddChild(tileRenderer);
        }
    }
}
