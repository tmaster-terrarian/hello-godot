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
	public static CameraManager CameraManager;

	[Export(PropertyHint.NodeType, "WorldEnvironment")]
	public WorldEnvironment WorldEnvironment;

	private World _world;

	public override void _Ready()
	{
		GetViewport().SetScaling3DMode(Viewport.Scaling3DModeEnum.Fsr2);
		GetViewport().SetScaling3DScale(1f);

		CameraManager = new CameraManager() { Name = "camera_manager" };
		AddChild(CameraManager);

		_world = new World { Name = "world", OnGenerateWorld = () => CameraManager.CenterCameraOnWorld(_world) };
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
}
