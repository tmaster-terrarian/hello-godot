using System;
using System.Collections.Generic;
using Godot;
using NewGameProject.Api;

namespace NewGameProject.Scripts;

public partial class World : Node3D
{
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
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        for (var i = 0; i < _tiles.Length; i++)
        {
            if (_tiles[i] is null) continue;

            if (GetTileIndex(Mathf.RoundToInt(_playerInstance.Position.X), Mathf.RoundToInt(_playerInstance.Position.Z)) == i)
                _tiles[i].IsStoodOn = true;
            else
                _tiles[i].IsStoodOn = false;
        }
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
                tile.QueueFree();
            }
        }
        _tiles = new Tile[Width * Height];

        for (var i = 0; i < levelData.Tiles.Length; i++)
        {
            var (tileIndex, tileMetadata) = MathUtil.Split(levelData.Tiles[i]);

            Tile node = MakeTile(tileIndex);
            if (node is null)
                continue;

            node.Metadata = tileMetadata;

            var pos = GetTilePosition(i);
            node.Position = new Vector3(pos.X, Random.Shared.NextSingle() * -5f - 1f, pos.Y);
            node.Name = $"tile_{pos.X}_{pos.Y}";

            _tiles[i] = node;

            AddChild(node);
        }

        if (_playerInstance is not null)
            _playerInstance.QueueFree();

        _playerInstance = new Player(StepOffTile, CanMoveToTile);
        _playerInstance.Position = new Vector3(levelData.PlayerLocation.X, 0.5f, levelData.PlayerLocation.Y);
        AddChild(_playerInstance);

        OnGenerateWorld?.Invoke();
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

            tileListTest[GetTileIndex(x, y)] = MathUtil.Join(metadata, (ushort)type);
        }

        GenerateLevel(new LevelData()
        {
            Width = Width,
            Height = Height,
            Tiles = tileListTest,
            PlayerLocation = new Vector2I(Random.Shared.Next(0, Width - 1), Random.Shared.Next(0, Height - 1))
        });
    }

    public int GetTileIndex(int x, int y) => y * Width + x;
    public int GetTileIndex(Vector2I pos) => GetTileIndex(pos.X, pos.Y);

    public Vector2I GetTilePosition(int index) => new(index % Width, index / Width);

    public bool IsPositionInMap(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
    public bool IsPositionInMap(Vector2I pos) => IsPositionInMap(pos.X, pos.Y);

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

    public Tile? GetTile(Vector2I pos)
    {
        if (!IsPositionInMap(pos)) return null;
        int tileIndex = GetTileIndex(pos.X, pos.Y);
        if (tileIndex < 0 || tileIndex >= _tiles.Length)
            return null;

        Tile targetTile = _tiles[tileIndex];
        return targetTile;
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
