using Godot;
using System;

public partial class NewScript : Node
{
	[Export]
	public Label3D Label { get; set; }

	public override void _Process(double delta)
	{
		base._Process(delta);

		if(Input.IsActionJustPressed("test_click"))
		{
			if(Label is not null)
			{
				Label.Text = "goodbye";
			}
		}
	}

	public override void _Ready()
	{
		base._Ready();
	}
}
