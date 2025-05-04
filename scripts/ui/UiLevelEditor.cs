using Godot;
using System;
using NewGameProject.Api;

namespace NewGameProject.Scripts.UI;
public partial class UiLevelEditor : Control
{
    public LevelEditor LevelEditor { get; set; }

    [Export]
    public Control BottomPanel { get; set; }
    [Export]
    public Container TilesListNode { get; set; }
    [Export]
    public Control EditorClickArea { get; set; }

    public ushort[] TileIds { get; set; }
    public Action OnEditorClicked;

    private bool _showBottomPanel = true;
    private bool _isDrawingTiles = false;

    public override void _Ready()
    {
        base._Ready();

        foreach (var tileId in TileIds)
        {
            var button = new LevelEditorTileButton
            {
                TileId = tileId,
                CustomMinimumSize = new Vector2(200, 200),
            };
            button.GuiInput += inputEvent => TileInputEvent(inputEvent, tileId);
            LevelEditor.OnChangeSelection += button.OnChangeSelection;
            TilesListNode.AddChild(button);
        }

        EditorClickArea.GuiInput += EditorClickAreaInput;

        LevelEditor.SetBrushTile(LevelEditor.TileBrushData);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!Visible) return;

        BottomPanel.OffsetBottom = MathUtil.ExpDecay(
            BottomPanel.OffsetBottom,
            _showBottomPanel ? 0 : BottomPanel.Size.Y,
            16f,
            (float)delta
        );

        var mousePos = GetViewport().GetMousePosition();
        if (_isDrawingTiles && mousePos.Y > BottomPanel.GlobalPosition.Y - 200f)
        {
            _showBottomPanel = false;
        }

        if (Input.IsActionJustReleased("editor_add"))
        {
            _showBottomPanel = true;
            _isDrawingTiles = false;
        }
    }

    private void TileInputEvent(InputEvent inputEvent, ushort tileId)
    {
        if (inputEvent is not InputEventMouseButton mouseButton) return;
        if (!mouseButton.IsReleased()) return;
        LevelEditor.SetBrushTile(tileId);
    }

    private void EditorClickAreaInput(InputEvent inputEvent)
    {
        if (!Input.IsActionPressed("editor_add")) return;
        _isDrawingTiles = true;
        OnEditorClicked?.Invoke();
    }
}
