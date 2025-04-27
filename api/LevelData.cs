using Godot;

namespace NewGameProject.Api;

public struct LevelData
{
    public uint[] Tiles { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Vector2I PlayerLocation { get; set; }

    public int GetTileIndex(int x, int y) => y * Width + x;
    public int GetTileIndex(Vector2I pos) => GetTileIndex(pos.X, pos.Y);

    public Vector2I GetTilePosition(int index) => new(index % Width, index / Width);

    public bool IsPositionInMap(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
    public bool IsPositionInMap(Vector2I pos) => IsPositionInMap(pos.X, pos.Y);
}
