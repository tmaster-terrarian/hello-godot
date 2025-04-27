using System;
using Godot;
using NewGameProject.Api;

namespace NewGameProject.Scripts;

public partial class LevelEditor(World world) : Node3D
{
    private PackedScene _selectionBoxModel = GD.Load<PackedScene>("res://models/selection_box.glb");

    public bool IsPlayMode { get; private set; }

    private LevelData _levelData;

    private Node3D _selectionBox;
    private Vector2I _selectionBoxPosition;

    private float _time;

    public override void _Ready()
    {
        base._Ready();

        _levelData = new LevelData()
        {
            Width = 4,
            Height = 4,
            Tiles = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
            PlayerLocation = Vector2I.Zero,
        };

        _selectionBox = _selectionBoxModel.Instantiate<Node3D>();
        AddChild(_selectionBox);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        _selectionBox.Position = MathUtil.ExpDecay(
            _selectionBox.Position,
            new Vector3(_selectionBoxPosition.X, 0.25f, _selectionBoxPosition.Y),
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
                world.SpawnPlayer();
            }
            else
            {
                world.DespawnPlayer();
                world.LevelData = _levelData;
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

        _selectionBoxPosition += inputDir;

        HandleEdits();
    }

    private void HandleEdits()
    {
        int tileIndex = _levelData.GetTileIndex(_selectionBoxPosition);
        if (!_levelData.IsPositionInMap(_selectionBoxPosition)) return;

        uint addIndex = 999;
        if (Input.IsKeyPressed(Key.Key0)) addIndex = 0;
        if (Input.IsKeyPressed(Key.Key1)) addIndex = 1;
        if (Input.IsKeyPressed(Key.Key2)) addIndex = 2;
        if (Input.IsKeyPressed(Key.Key3)) addIndex = 3;

        if (addIndex < 999)
        {
            if (_levelData.Tiles[tileIndex] == addIndex) return;
            _levelData.Tiles[tileIndex] = addIndex;

            world.GetTile(tileIndex)?.EditorDelete();
            var newNode = world.GenerateTile(tileIndex, addIndex);

            newNode?.EditorCreate();
        }
    }
}
