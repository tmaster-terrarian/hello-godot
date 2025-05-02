using System;
using System.Collections.Generic;
using Godot;
using NewGameProject.Api;

namespace NewGameProject.Scripts;

public partial class World : Node3D
{
    public LevelData LevelData { get; private set; }

    public Action OnGenerateWorld;

    private Node3D _tileNodes;
    private Tile[] _tiles;

    public int Width => LevelData.Width;
    public int Height => LevelData.Height;

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

        AddChild(new LevelEditor(this) { Name = "levelEditor" });
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_playerInstance is null || !IsInstanceValid(_playerInstance)) return;
        for (var i = 0; i < _tiles.Length; i++)
        {
            var tile = _tiles[i];
            if (tile is null) continue;
            if (!IsInstanceValid(tile)) continue;
            tile.IsStoodOn = LevelData.GetTileIndex(Mathf.RoundToInt(_playerInstance.Position.X), Mathf.RoundToInt(_playerInstance.Position.Z)) == i;
        }
    }

    public void SetLevelData(LevelData levelData)
    {
        LevelData = levelData;
    }

    public void GenerateLevel(LevelData levelData)
    {
        LevelData = levelData;

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
        var width = Random.Shared.Next(3, 9);
        var height = Random.Shared.Next(3, 9);
        uint[] tileListTest = new uint[width * height];
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
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
            playerLocation = new Vector2I(Random.Shared.Next(0, width - 1), Random.Shared.Next(0, height - 1));
        } while (tileListTest[LevelData.GetTileIndex(playerLocation.X, playerLocation.Y)] == 0);

        var levelData = new LevelData()
        {
            Width = width,
            Height = height,
            Tiles = tileListTest,
            PlayerLocation = playerLocation
        };
        GenerateLevel(levelData);
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

        _playerInstance = new Player(this);
        _playerInstance.Position = new Vector3(LevelData.PlayerLocation.X, 0.5f, LevelData.PlayerLocation.Y);
        AddChild(_playerInstance);
    }

    public void DespawnPlayer()
    {
        if (_playerInstance is not null)
            _playerInstance.QueueFree();
        _playerInstance = null;
    }

    public static Tile? MakeTile(TileTypes type)
    {
        return type switch
        {
            TileTypes.Solid => new TileSolid(),
            TileTypes.Fall => new TileFall(),
            TileTypes.Bridge => new TileBridge(),
            _ => null
        };
    }

    public static Tile? MakeTile(int typeIndex)
    {
        return MakeTile((TileTypes)typeIndex);
    }

    public static Tile? MakeTile(uint typeIndex)
    {
        var (tileIndex, tileMetadata) = MathUtil.Split(typeIndex);
        return MakeTile((TileTypes)tileIndex)?.SetMetadata(tileMetadata);
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

    public void StepOnTile(Vector2I tilePosition)
    {
        var targetTile = GetTile(tilePosition);
        targetTile?.OnStepOn();
    }

    public void StepOffTile(Vector2I tilePosition)
    {
        var targetTile = GetTile(tilePosition);
        targetTile?.OnStepOff();
    }
}
