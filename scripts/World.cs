using System;
using System.Collections.Generic;
using Godot;
using NewGameProject.Api;

namespace NewGameProject.Scripts;

public partial class World : Node3D
{
    private LevelData _levelData;
    public LevelData LevelData
    {
        get => _levelData;
        set
        {
            _levelData = value;
            GenerateLevel(value);
        }
    }

    public Action OnGenerateWorld;

    private Node3D _tileNodes;
    private Tile[] _tiles;

    [Export]
    public int Width { get; set; } = 9;

    [Export]
    public int Height { get; set; } = 9;

    private Node3D _playerInstance;

    public enum TileTypes
    {
        Invalid,
        Solid,
        Fall,
        Bridge,
    }

    public override void _Ready()
    {
        _tileNodes = new Node3D() { Name = "tiles" };

        GenerateRandomLevel();

        AddChild(new LevelEditor(this));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        for (var i = 0; i < _tiles.Length; i++)
        {
            if (_tiles[i] is null) continue;

            if (LevelData.GetTileIndex(Mathf.RoundToInt(_playerInstance.Position.X), Mathf.RoundToInt(_playerInstance.Position.Z)) == i)
                _tiles[i].IsStoodOn = true;
            else
                _tiles[i].IsStoodOn = false;
        }
    }

    public void SetLevelData(LevelData levelData)
    {
        LevelData = levelData;
    }

    public void GenerateLevel(LevelData levelData)
    {
        Width = levelData.Width;
        Height = levelData.Height;

        if (_tiles is not null)
        {
            foreach (var tile in _tiles)
            {
                if (tile is null) continue;
                if (!IsInstanceValid(tile)) continue;
                tile.QueueFree();
            }
        }
        _tiles = new Tile[Width * Height];

        for (var i = 0; i < levelData.Tiles.Length; i++)
        {
            GenerateTile(i, levelData.Tiles[i]);
        }

        OnGenerateWorld?.Invoke();
    }

    public Tile? GenerateTile(int index, uint tileData)
    {
        var (tileIndex, tileMetadata) = MathUtil.Split(tileData);

        if (_tiles.GetValue(index) is Tile existingTile)
        {
            if (IsInstanceValid(existingTile) && !existingTile.IsDeleting)
                existingTile.QueueFree();
        }

        Tile node = MakeTile(tileIndex);
        if (node is null)
            return null;

        node.Metadata = tileMetadata;

        var pos = LevelData.GetTilePosition(index);
        node.Position = new Vector3(pos.X, 0f, pos.Y);
        node.Name = $"tile_{pos.X}_{pos.Y}";

        _tiles[index] = node;

        AddChild(node);

        return node;
    }

    public void GenerateRandomLevel()
    {
        Width = Random.Shared.Next(3, 9);
        Height = Random.Shared.Next(3, 9);
        uint[] tileListTest = new uint[Width * Height];
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            int tileTypeIndex = Random.Shared.Next(0, 4);
            var type = (TileTypes)tileTypeIndex;

            ushort metadata = ushort.MinValue;
            if (type == TileTypes.Bridge)
                metadata = (ushort)Random.Shared.Next();

            tileListTest[LevelData.GetTileIndex(x, y)] = MathUtil.Join(metadata, (ushort)type);
        }

        Vector2I playerLocation;
        do
        {
            playerLocation = new Vector2I(Random.Shared.Next(0, Width - 1), Random.Shared.Next(0, Height - 1));
        } while (tileListTest[LevelData.GetTileIndex(playerLocation.X, playerLocation.Y)] == 0);

        LevelData = new LevelData()
        {
            Width = 4,
            Height = 4,
            Tiles = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
            PlayerLocation = playerLocation
        };
    }

    public void StartLevel()
    {
        foreach (var tile in _tiles)
        {
            tile.Position = tile.Position with { Y = Random.Shared.NextSingle() * -5f - 1f };
        }

        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        DespawnPlayer();

        _playerInstance = new Player(StepOffTile, CanMoveToTile);
        _playerInstance.Position = new Vector3(LevelData.PlayerLocation.X, 0.5f, LevelData.PlayerLocation.Y);
        AddChild(_playerInstance);
    }

    public void DespawnPlayer()
    {
        if (_playerInstance is not null)
            _playerInstance.QueueFree();
        _playerInstance = null;
    }

    public Tile? MakeTile(TileTypes type)
    {
        return type switch
        {
            TileTypes.Solid => new TileSolid(),
            TileTypes.Fall => new TileFall(),
            TileTypes.Bridge => new TileBridge(),
            _ => null
        };
    }

    public Tile? MakeTile(int typeIndex)
    {
        return MakeTile((TileTypes)typeIndex);
    }

    public Tile? GetTile(int index)
    {
        if (index < 0 || index >= _tiles.Length)
            return null;

        Tile targetTile = _tiles[index];
        if (!IsInstanceValid(targetTile)) return null;
        return targetTile;
    }

    public Tile? GetTile(Vector2I pos)
    {
        if (!LevelData.IsPositionInMap(pos)) return null;
        int tileIndex = LevelData.GetTileIndex(pos.X, pos.Y);
        return GetTile(tileIndex);
    }

    public bool CanMoveToTile(Vector2I posFrom, Vector2I posTo)
    {
        var fromTile = GetTile(posFrom);
        var toTile = GetTile(posTo);
        if (toTile is null) return false;
        if (fromTile is not null && !fromTile.CanStepOff(posFrom - posTo)) return false;
        if (!toTile.CanStepOn(posTo - posFrom)) return false;
        return true;
    }

    public void StepOffTile(Vector2I tilePosition)
    {
        var targetTile = GetTile(tilePosition);
        targetTile?.Interact();
    }
}
