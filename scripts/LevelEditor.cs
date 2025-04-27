using System;
using Godot;
using NewGameProject.Api;

namespace NewGameProject.Scripts;

public partial class LevelEditor(World world) : Node3D
{
    private PackedScene _selectionBoxModel = GD.Load<PackedScene>("res://models/selection_box.glb");
    private PackedScene _playerSpawnerModel = GD.Load<PackedScene>("res://models/player.glb");
    private PackedScene _gridSquareModel = GD.Load<PackedScene>("res://models/grid_square.glb");

    public bool IsPlayMode { get; private set; }

    private LevelData _levelData;

    private Node3D _selectionBox;
    private Vector2I _selectionBoxPosition;

    private Node3D _playerSpawner;
    private Node3D _grid;

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

        _playerSpawner = _playerSpawnerModel.Instantiate<Node3D>();
        var animationPlayer = _playerSpawner.GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Play("walk");
        animationPlayer.Advance(0);
        animationPlayer.Pause();
        _playerSpawner.GetNode<Node3D>("RoboBoy/Skeleton3D/Ice Head").SetVisible(false);
        _playerSpawner.GetNode<Node3D>("RoboBoy/Skeleton3D/Water Head").SetVisible(false);
        _playerSpawner.GetNode<Node3D>("Pick").SetVisible(false);
        AddChild(_playerSpawner);

        _grid = Draw3D.Grid(-Vector3.One * 0.5f, _levelData.Width, _levelData.Height, new Color(Colors.White, 0.25f));
        _grid.RotateX(Mathf.DegToRad(90));
        AddChild(_grid);
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

        uint addIndex = uint.MaxValue;
        if (Input.IsKeyPressed(Key.Key0)) addIndex = 0;
        if (Input.IsKeyPressed(Key.Key1)) addIndex = 1;
        if (Input.IsKeyPressed(Key.Key2)) addIndex = 2;
        if (Input.IsKeyPressed(Key.Key3)) addIndex = MathUtil.Join(0, 3);
        if (Input.IsKeyPressed(Key.Key4)) addIndex = MathUtil.Join(1, 3);

        if (addIndex < uint.MaxValue)
        {
            if (_levelData.Tiles[tileIndex] == addIndex) return;
            _levelData.Tiles[tileIndex] = addIndex;

            world.GetTile(tileIndex)?.EditorDelete();
            var newNode = world.GenerateTile(tileIndex, addIndex);

            newNode?.EditorCreate();
        }
    }
}
