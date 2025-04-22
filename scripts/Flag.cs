using Godot;
using System;

public partial class Flag : Node3D
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public const string AnimAppear = "flag_appear";
    public const string AnimRemove = "flag_remove";
}
