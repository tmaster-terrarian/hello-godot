using Godot;
using System;

namespace NewGameProject.Scripts.UI;
public partial class UiLevelEditor : PanelContainer
{
    public LevelEditor LevelEditor { get; set; }
    private PackedScene _uiTileRendererScene = GD.Load<PackedScene>("res://scenes/ui/ui_tile_renderer.tscn");

    [Export]
    public Container TilesListNode { get; set; }
    public ushort[] TileIds { get; set; }

    public override void _Ready()
    {
        base._Ready();

        foreach (var tileId in TileIds)
        {
            SubViewportContainer subViewportContainer = new SubViewportContainer()
            {
                Stretch = true,
                CustomMinimumSize = new Vector2(200, 200),
            };
            TilesListNode.AddChild(subViewportContainer);
            subViewportContainer.Connect("gui_input",
                Callable.From<InputEvent>((inputEvent) =>
                {
                    if (!inputEvent.IsActionReleased("test_click", true)) return;
                    LevelEditor.SetBrushTile(tileId);
                })
            );

            var tileRenderer = _uiTileRendererScene.Instantiate<UiTileRenderer>();
            tileRenderer.Contents = World.MakeTile(tileId);
            subViewportContainer.AddChild(tileRenderer);
        }
    }
}
