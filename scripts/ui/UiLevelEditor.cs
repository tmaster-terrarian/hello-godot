using Godot;
using System;

namespace NewGameProject.Scripts.UI;
public partial class UiLevelEditor : Control
{
    public LevelEditor LevelEditor { get; set; }
    private PackedScene _uiTileRendererScene = GD.Load<PackedScene>("res://scenes/ui/ui_tile_renderer.tscn");

    [Export]
    public Container TilesListNode { get; set; }
    [Export]
    public Control EditorClickArea { get; set; }

    public ushort[] TileIds { get; set; }
    public Action OnEditorClicked;

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
            subViewportContainer.GuiInput += inputEvent => TileInputEvent(inputEvent, tileId);

            var tileRenderer = _uiTileRendererScene.Instantiate<UiTileRenderer>();
            tileRenderer.Contents = World.MakeTile(tileId);
            subViewportContainer.AddChild(tileRenderer);
        }

        EditorClickArea.GuiInput += EditorClickAreaInput;
    }

    private void TileInputEvent(InputEvent inputEvent, ushort tileId)
    {
        if (!inputEvent.IsActionReleased("test_click", true)) return;
        LevelEditor.SetBrushTile(tileId);
    }

    private void EditorClickAreaInput(InputEvent inputEvent)
    {
        if (!Input.IsActionPressed("editor_add")) return;
        OnEditorClicked?.Invoke();
    }
}
