using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NewGameProject.scripts;

public partial class Main : Node
{
	private Node3D _tileGrid;

	private PackedScene _tile = GD.Load<PackedScene>("res://scenes/tile.tscn");

	private Node3D _playerInstance;

	private bool[,] _bomb;
	private bool[,] _flagged;
	private Tile[,] _tiles;
	private Vector2I[] _bombPositions;

	[Export]
	public int Width { get; set; } = 9;

	[Export]
	public int Height { get; set; } = 9;

	[Export]
	public Material Material1;

	[Export]
	public Material Material2;

	public override void _Ready()
	{
		GetViewport().SetScaling3DMode(Viewport.Scaling3DModeEnum.Fsr2);
		GetViewport().SetScaling3DScale(1f);

		_bomb = new bool[Width, Height];
		_flagged = new bool[Width, Height];
		_tiles = new Tile[Width, Height];

		int bombs = 10;
		_bombPositions = new Vector2I[bombs];
		Array.Fill(_bombPositions, new(-1, -1));

		_tileGrid = new()
		{
			Name = "tiles"
		};

		_tileGrid.TreeEntered += () =>
		{
			for (int x = -Width / 2; x < (Width + 1) / 2; x++)
			{
				for (int z = -Height / 2; z < (Height + 1) / 2; z++)
				{
					var node = _tile.Instantiate<Node3D>();
					Vector3 pos = new(x, 0, z);

					if (((x + Width / 2) % 2 == 1) ^ ((z + Height / 2) % 2 == 1))
					{
						node.GetChild(0).GetChild<MeshInstance3D>(0, true).MaterialOverride = Material1;
					}
					else
					{
						node.GetChild(0).GetChild<MeshInstance3D>(0, true).MaterialOverride = Material2;
					}

					if (Width % 2 == 0)
						pos.X += 0.5f;
					if (Height % 2 == 0)
						pos.Z += 0.5f;

					node.Position = pos;
					node.RotateY((float)GD.RandRange(-0.1, 0.1));
					node.Name = $"tile_{x + Width / 2}_{z + Height / 2}";

					_tiles[x + Width / 2, z + Height / 2] = (Tile)node;
					_tileGrid.AddChild(node);
				}
			}

			List<Vector2I> bombPositions = [];
			for (int i = 0; i < bombs; i++)
			{
				Vector2I pos;

				do
				{
					pos = new(Random.Shared.Next(Width), Random.Shared.Next(Height));
				}
				while (bombPositions.Contains(pos));

				bombPositions.Add(pos);

				_bomb[pos.X, pos.Y] = true;
				_flagged[pos.X, pos.Y] = true;
				_tiles[pos.X, pos.Y].FlagActive = true;
			}
		};

		AddChild(_tileGrid);

		_playerInstance = new Player(StepOffTile, CanMoveToTile);
		_playerInstance.Position = new Vector3(0, 0.5f, 0);
		AddChild(_playerInstance);
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("test_click"))
		{
			bool found = false;
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					if (_flagged[x, y])
					{
						_flagged[x, y] = false;
						_tiles[x, y].FlagActive = false;
						found = true;
						break;
					}
				}
				if (found)
					break;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{

	}

	private Tile? GetTile(Vector2I pos)
	{
		if (pos.X < 0 || pos.X >= _tiles.GetLength(0))
			return null;
		if (pos.Y < 0 || pos.Y >= _tiles.GetLength(1))
			return null;

		Tile targetTile = (Tile)_tiles.GetValue(pos.X, pos.Y);
		return targetTile;
	}

	private bool CanMoveToTile(Vector2I pos)
	{
		var targetTile = GetTile(pos);
		if (targetTile is null) return false;
		if (!targetTile.IsAlive) return false;
		return true;
	}

	private void StepOffTile(Vector2I tilePosition)
	{
		var targetTile = GetTile(tilePosition);
		targetTile?.Interact();
	}
}
