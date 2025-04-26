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

	private Node3D _playerInstance;

	private Tile[,] _tiles;
	private Vector2I[] _bombPositions;

	[Export]
	public int Width { get; set; } = 9;

	[Export]
	public int Height { get; set; } = 9;

	public override void _Ready()
	{
		GetViewport().SetScaling3DMode(Viewport.Scaling3DModeEnum.Fsr2);
		GetViewport().SetScaling3DScale(1f);

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
					var id = Random.Shared.Next(0, 3);
					Tile node = new Tile();
					switch (id)
					{
						case 0:
							node = new TileSolid();
							break;
						case 1:
							node = new TileFall();
							break;
						case 2:
							node = new TileBridge();
							break;
					}

					Vector3 pos = new(x, 0, z);

					if (((x + Width / 2) % 2 == 1) ^ ((z + Height / 2) % 2 == 1))
						node.ShowAlternate = true;

					if (Width % 2 == 0)
						pos.X += 0.5f;
					if (Height % 2 == 0)
						pos.Z += 0.5f;

					node.Position = pos;
					node.Name = $"tile_{x + Width / 2}_{z + Height / 2}";

					_tiles[x + Width / 2, z + Height / 2] = node;
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
			}
		};

		AddChild(_tileGrid);

		_playerInstance = new Player(StepOffTile, CanMoveToTile);
		_playerInstance.Position = new Vector3(0, 0.5f, 0);
		AddChild(_playerInstance);
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

	private bool CanMoveToTile(Vector2I posFrom, Vector2I posTo)
	{
		var fromTile = GetTile(posFrom);
		var toTile = GetTile(posTo);
		if (toTile is null) return false;
		if (fromTile is not null && !fromTile.CanStepOff(posFrom - posTo)) return false;
		if (!toTile.CanStepOn(posTo - posFrom)) return false;
		return true;
	}

	private void StepOffTile(Vector2I tilePosition)
	{
		var targetTile = GetTile(tilePosition);
		targetTile?.Interact();
	}
}
