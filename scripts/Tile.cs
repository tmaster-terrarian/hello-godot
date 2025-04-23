using Godot;
using System;

public partial class Tile : Node3D
{
    [Export]
    public Node3D Block { get; set; }

    [Export]
    public Flag Flag { get; set; }

    public event Action FlagActiveChanged;

    private bool _flagActive = false;

    [Export]
    public bool FlagActive
    {
        get => _flagActive;
        set
        {
            if (_flagActive == value)
                return;

            _flagActive = value;
            Flag.AnimationPlayer.CurrentAnimation = _flagActive
                ? Flag.AnimAppear : Flag.AnimRemove;

            FlagActiveChanged?.Invoke();
        }
    }
}
