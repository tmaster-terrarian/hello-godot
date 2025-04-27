using Godot;

namespace NewGameProject.Api;

public struct LevelData
{
    public int[] Tiles { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Vector2I PlayerLocation { get; set; }
}
