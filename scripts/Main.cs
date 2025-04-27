using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NewGameProject.Api;
using NewGameProject.Scripts;

public partial class Main : Node
{
	[Export(PropertyHint.NodeType, "Node3D")]
	public Node3D CameraRoot;

	[Export(PropertyHint.NodeType, "WorldEnvironment")]
	public WorldEnvironment WorldEnvironment;

	private World _world;

	public override void _Ready()
	{
		GetViewport().SetScaling3DMode(Viewport.Scaling3DModeEnum.Fsr2);
		GetViewport().SetScaling3DScale(1f);

		_world = new World { Name = "world", OnGenerateWorld = CenterCameraOnWorld };
		AddChild(_world);
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("test_click"))
		{
			WorldEnvironment.Environment.BackgroundColor.ToHsv(out _, out float saturation,
				out float value);
			WorldEnvironment.Environment.BackgroundColor =
				Color.FromHsv(Random.Shared.NextSingle(), saturation, value);
		}
	}

	public void CenterCameraOnWorld()
	{
		CameraRoot.Position = new Vector3((float)_world.Width / 2 - 0.5f, 0, (float)_world.Height / 2 - 0.5f);
	}
}
