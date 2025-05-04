using Godot;
using NewGameProject.Api;

namespace NewGameProject.Scripts.UI;

public partial class LevelEditorTileButton : LevelEditorButton<ushort>
{
    private PackedScene _uiTileRendererScene = GD.Load<PackedScene>("res://scenes/ui/ui_tile_renderer.tscn");

    public required ushort TileId { get; set; }

    private bool _isHovered = false;
    private float _hoverTime = 0;

    private float _selectionTime = 0;

    private UiTileRenderer _uiTileRenderer;
    private DropShadow _dropShadow;

    public override void _Ready()
    {
        base._Ready();

        MouseEntered += () => { _isHovered = true; };
        MouseExited += () => { _isHovered = false; };

        var subViewportContainer = new SubViewportContainer()
        {
            Stretch = true,
            CustomMinimumSize = new Vector2(200, 200),
        };
        AddChild(subViewportContainer);

        _uiTileRenderer = _uiTileRendererScene.Instantiate<UiTileRenderer>();
        _uiTileRenderer.Contents = World.MakeTile(TileId);
        subViewportContainer.AddChild(_uiTileRenderer);

        _dropShadow = new DropShadowViewport()
        {
            Viewport = _uiTileRenderer,
            TargetNode = this,
            Offset = Vector2.One * 10f,
            Color = Colors.Black * 0.4f,
        };
        AddChild(_dropShadow);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Scale = MathUtil.ExpDecay(Scale, _isHovered ? Vector2.One * 1.5f : Vector2.One, 16f, (float)delta);

        if (_isHovered)
        {
            _hoverTime += (float)delta;
            Position = Position with { Y = MathUtil.ExpDecay(Position.Y, -50, 16f, (float)delta) };
        }
        else
        {
            _hoverTime = 0;
            Position = Position with { Y = MathUtil.ExpDecay(Position.Y, 0, 16f, (float)delta) };
        }

        if (IsSelected)
        {
            _selectionTime += (float)delta;
            RotationDegrees = MathUtil.ExpDecay(RotationDegrees, Mathf.Sin(_selectionTime * 10f) * 15f, 16f, (float)delta);
        }
        else
        {
            _selectionTime = 0;
            RotationDegrees = 0;
        }
    }

    public override void OnChangeSelection(ushort oldValue, ushort newValue)
    {
        base.OnChangeSelection(oldValue, newValue);

        if (newValue == TileId)
        {
            IsSelected = true;
        }
        else
        {
            IsSelected = false;
        }
    }
}
