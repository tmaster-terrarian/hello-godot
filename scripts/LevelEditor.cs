using System;
using System.Collections.Generic;
using Godot;
using NewGameProject.Api;
using NewGameProject.Scripts.UI;

namespace NewGameProject.Scripts;

public partial class LevelEditor(World world) : Node3D
{
    [Signal]
    public delegate void OnChangeSelectionEventHandler(ushort oldValue, ushort newValue);

    private PackedScene _uiScene = GD.Load<PackedScene>("res://scenes/ui/ui_leveleditor.tscn");
    private PackedScene _selectionBoxModel = GD.Load<PackedScene>("res://models/selection_box.glb");
    private PackedScene _playerSpawnerModel = GD.Load<PackedScene>("res://models/player.glb");
    private readonly ShaderMaterial _gridMaterial = GD.Load<ShaderMaterial>("res://materials/editor_grid.tres");
    public bool IsPlayMode { get; private set; }

    private LevelData _levelData;

    private ushort _tileBrushData = 1;
    public ushort TileBrushData { get => _tileBrushData; private set => _tileBrushData = value; }

    private Node3D _selectionBox;
    private Vector2I _selectionBoxPosition;

    private Node3D _playerSpawner;
    private Node3D _grid;

    private float _time;
    private Viewport _viewport;
    private Camera3D _camera3D;

    private UiLevelEditor _uiEditor;

    public override void _Ready()
    {
        base._Ready();

        _viewport = GetViewport();
        _camera3D = _viewport.GetCamera3D();

        // int w = 18;
        // int h = 13;
        int w = 127;
        int h = 127;
        _levelData = new LevelData()
        {
            Width = w,
            Height = h,
            Tiles = new ushort[w * h],
            PlayerLocation = new Vector2(w * 0.5f, h * 0.5f).ToVector2I(),
        };

        _selectionBoxPosition = new Vector2(w * 0.5f, h * 0.5f).ToVector2I();

        _selectionBox = _selectionBoxModel.Instantiate<Node3D>();
        AddChild(_selectionBox);
        _selectionBox.Position = _selectionBoxPosition.ToVector3(true, 0.25f);

        _playerSpawner = _playerSpawnerModel.Instantiate<Node3D>();
        var animationPlayer = _playerSpawner.GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Play("walk");
        animationPlayer.Advance(0);
        animationPlayer.Pause();
        _playerSpawner.GetNode<Node3D>("RoboBoy/Skeleton3D/Ice Head").SetVisible(false);
        _playerSpawner.GetNode<Node3D>("RoboBoy/Skeleton3D/Water Head").SetVisible(false);
        _playerSpawner.GetNode<Node3D>("Pick").SetVisible(false);
        AddChild(_playerSpawner);
        _playerSpawner.Position = _levelData.PlayerLocation.ToVector3(true, 0.5f);

        _grid = Draw3D.Grid(
            -Vector3.One * 0.5f,
            _levelData.Width,
            _levelData.Height,
            new Color(Colors.White, 0.5f),
            _gridMaterial
        );
        _grid.RotateX(Mathf.DegToRad(90));
        AddChild(_grid);

        world.GenerateLevel(_levelData);

        _uiEditor = _uiScene.Instantiate<UiLevelEditor>();
        _uiEditor.LevelEditor = this;
        _uiEditor.TileIds =
        [
            1,
            2,
            3,
            0b_00000001_00000011,
            MathUtil.Join(0b10, 3),
            MathUtil.Join(0b11, 3),
        ];
        _uiEditor.OnEditorClicked = OnEditorClicked;
        AddChild(_uiEditor);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        RenderingServer.GlobalShaderParameterSet("editor_highlight_position", _selectionBox.Position);

        if (Input.GetLastMouseVelocity().LengthSquared() > 0.1f)
        {
            var mousePosition = _viewport.GetMousePosition();
            var from = _camera3D.ProjectRayOrigin(mousePosition);
            var dir = from + _camera3D.ProjectRayNormal(mousePosition) - from;
            var plane = new Plane(Vector3.Up, Vector3.Up * 0.25f);
            if (plane.IntersectsRay(from, dir) is { } hitPos)
            {
                _selectionBoxPosition = new Vector2I(MathUtil.RoundToInt(hitPos.X), MathUtil.RoundToInt(hitPos.Z));
            }
        }

        _selectionBox.Position = MathUtil.ExpDecay(
            _selectionBox.Position,
            _selectionBoxPosition.ToVector3(true, 0.25f),
            16f,
            (float)delta
        );

        float scaleIntensity = 0.05f;
        float timeSpedUp = _time * 8f;
        _selectionBox.Scale = new Vector3(
            1f + Mathf.Sin(timeSpedUp) * scaleIntensity,
            1f,
            1f + Mathf.Sin(timeSpedUp + 2.1523f) * scaleIntensity
        );

        _time += (float)delta;

        var spawnPos = new Vector3(_levelData.PlayerLocation.X, 0.5f, _levelData.PlayerLocation.Y);
        _playerSpawner.Position = MathUtil.ExpDecay(_playerSpawner.Position, spawnPos, 16f, (float)delta);
        _playerSpawner.SetVisible(!IsPlayMode);
        _grid.SetVisible(!IsPlayMode);
        _selectionBox.SetVisible(!IsPlayMode);
        if (!IsPlayMode)
        {
            HandleEditMode();
        }

        if (Input.IsActionJustPressed("editor_toggle_playmode"))
        {
            IsPlayMode = !IsPlayMode;
            if (IsPlayMode)
            {
                world.SetLevelData(_levelData);
                world.SpawnPlayer();
                _uiEditor.SetVisible(false);
            }
            else
            {
                world.DespawnPlayer();
                world.GenerateLevel(_levelData);
                _uiEditor.SetVisible(true);
            }
        }
    }

    private void HandleEditMode()
    {
        var inputDir = Vector2I.Zero;
        if (Input.IsActionJustPressed("move_east"))
            inputDir = Vector2I.Right;
        else if (Input.IsActionJustPressed("move_west"))
            inputDir = Vector2I.Left;
        else if (Input.IsActionJustPressed("move_south"))
            inputDir = Vector2I.Down;
        else if (Input.IsActionJustPressed("move_north"))
            inputDir = Vector2I.Up;

        _selectionBox.Position += inputDir.ToVector3(true, 0f) * 0.2f;
        _selectionBoxPosition += inputDir;
        _selectionBoxPosition.X = MathUtil.ClampToInt(_selectionBoxPosition.X, 0, _levelData.Width - 1);
        _selectionBoxPosition.Y = MathUtil.ClampToInt(_selectionBoxPosition.Y, 0, _levelData.Height - 1);

        HandleEdits();
    }

    private void HandleEdits()
    {
        if (!_levelData.IsPositionInMap(_selectionBoxPosition)) return;
        int tileIndex = _levelData.GetTileIndex(_selectionBoxPosition);

        if (Input.IsKeyPressed(Key.R))
        {
            _levelData.PlayerLocation = _selectionBoxPosition;
        }

        if (Input.IsActionJustPressed("editor_toggle_camera"))
        {
            int newCamState = Mathf.Wrap(
                (int)Main.CameraManager.CameraState + 1,
                0,
                Enum.GetNames(typeof(CameraManager.CameraStates)).Length
            );
            Main.CameraManager.CameraState = (CameraManager.CameraStates)newCamState;
        }

        ushort? addIndex = null;
        if (Input.IsKeyPressed(Key.Key1)) addIndex = 1;
        if (Input.IsKeyPressed(Key.Key2)) addIndex = 2;
        if (Input.IsKeyPressed(Key.Key3)) addIndex = MathUtil.Join(0b00, 3);
        if (Input.IsKeyPressed(Key.Key4)) addIndex = MathUtil.Join(0b01, 3);
        if (Input.IsKeyPressed(Key.Key5)) addIndex = MathUtil.Join(0b10, 3);
        if (Input.IsKeyPressed(Key.Key6)) addIndex = MathUtil.Join(0b11, 3);
        if (addIndex is not null) SetBrushTile(addIndex.Value);

        if (!Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (Input.IsActionPressed("editor_add"))
            {
                PaintTile(tileIndex, _tileBrushData);
            }

            if (Input.IsActionPressed("editor_remove"))
            {
                PaintTile(tileIndex, 0);
            }
        }
    }

    public void SetBrushTile(ushort data)
    {
        var oldValue = _tileBrushData;
        TileBrushData = data;
        EmitSignalOnChangeSelection(oldValue, data);
    }

    private void PaintTile(int tileIndex, ushort tileBrushId)
    {
        if (_levelData.Tiles[tileIndex] == tileBrushId) return;
        _levelData.Tiles[tileIndex] = tileBrushId;

        world.GetTile(tileIndex)?.EditorDelete();
        var newNode = world.GenerateTile(tileIndex, tileBrushId);

        newNode?.EditorCreate();
    }

    private void OnEditorClicked()
    {
        int tileIndex = _levelData.GetTileIndex(_selectionBoxPosition);
        PaintTile(tileIndex, _tileBrushData);
    }
}
