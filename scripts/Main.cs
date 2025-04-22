using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Main : Node
{
    private Node3D _tileGrid;

    private PackedScene _tile = GD.Load<PackedScene>("res://scenes/tile.tscn");

    private bool[,] _bomb;
    private bool[,] _flagged;
    private Tile[,] _tiles;
    private Vector2I[] _bombPositions;

    [Export]
    public int Width { get; set; } = 9;

    [Export]
    public int Height { get; set; } = 9;

    public override void _Ready()
    {
        _bomb = new bool[Width, Height];
        _flagged = new bool[Width, Height];
        _tiles = new Tile[Width, Height];

        int bombs = 10;
        _bombPositions = new Vector2I[bombs];
        Array.Fill(_bombPositions, new(-1, -1));

        _tileGrid = new() {
            Name = "tiles"
        };

        _tileGrid.TreeEntered += () => {
            for(int x = -Width / 2; x < (Width + 1) / 2; x++)
            {
                for(int z = -Height / 2; z < (Height + 1) / 2; z++)
                {
                    var node = _tile.Instantiate<Node3D>();
                    Vector3 pos = new(x, 0, z);

                    if(Width % 2 == 0)
                        pos.X += 0.5f;
                    if(Height % 2 == 0)
                        pos.Z += 0.5f;

                    node.Position = pos;
                    node.Name = $"tile_{x + Width / 2}_{z + Height / 2}";

                    _tiles[x + Width / 2, z + Height / 2] = (Tile)node;
                    _tileGrid.AddChild(node);
                }
            }

            List<Vector2I> bombPositions = [];
            for(int i = 0; i < bombs; i++)
            {
                Vector2I pos;

                do {
                    pos = new(Random.Shared.Next(Width), Random.Shared.Next(Height));
                }
                while(bombPositions.Contains(pos));

                bombPositions.Add(pos);

                _bomb[pos.X, pos.Y] = true;
                _flagged[pos.X, pos.Y] = true;
                _tiles[pos.X, pos.Y].FlagActive = true;
            }
        };

        AddChild(_tileGrid);

        // AddChild(_tile.Instantiate());
    }

    public override void _Process(double delta)
    {
        if(Input.IsActionJustPressed("test_click"))
        {
            bool found = false;
            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    if(_flagged[x, y])
                    {
                        _flagged[x, y] = false;
                        _tiles[x, y].FlagActive = false;
                        found = true;
                        break;
                    }
                }
                if(found)
                    break;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        
    }
}
