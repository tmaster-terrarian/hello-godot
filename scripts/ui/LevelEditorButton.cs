using System;
using Godot;
using NewGameProject.Api;

namespace NewGameProject.Scripts.UI;

public partial class LevelEditorButton<T> : Control
{
    public bool IsSelected;

    public override void _Ready()
    {
        base._Ready();

        SetPivotOffset(Size * 0.5f);
    }

    public virtual void OnChangeSelection(T oldValue, T newValue) {}
}
