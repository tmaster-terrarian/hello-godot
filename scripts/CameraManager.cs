using Godot;
using NewGameProject.Api;

namespace NewGameProject.Scripts;

public partial class CameraManager : Node3D
{
    public Node3D CameraRoot;
    public Camera3D CameraStandard;

    public enum CameraStates
    {
        Standard,
        TopDown,
    }

    public CameraStates CameraState = CameraStates.Standard;

    public override void _Ready()
    {
        base._Ready();

        CameraRoot = new Node3D()
        {
            Name = "camera_root",
            RotationDegrees = new Vector3(-65f, 0f, 0f),
        };
        AddChild(CameraRoot);

        CameraStandard = new Camera3D()
        {
            Name = "camera_standard",
            Projection = Camera3D.ProjectionType.Perspective,
            Fov = 22f,
            Position = new Vector3(0f, 0f, 37f),
        };
        CameraRoot.AddChild(CameraStandard);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        switch (CameraState)
        {
            case CameraStates.Standard:
                CameraRoot.Quaternion = MathUtil.ExpDecay(CameraRoot.Quaternion,
                    Quaternion.FromEuler(new Vector3(Mathf.DegToRad(-65f), 0f, 0f)), 16f, (float)delta);
                break;
            case CameraStates.TopDown:
                CameraRoot.Quaternion = MathUtil.ExpDecay(CameraRoot.Quaternion,
                    Quaternion.FromEuler(new Vector3(-Mathf.Pi * 0.5f, 0f, 0f)), 16f, (float)delta);
                break;
        }
    }

    public void CenterCameraOnWorld(World world)
    {
        CameraRoot.Position = new Vector3((float)world.Width / 2 - 0.5f, 0, (float)world.Height / 2 - 0.5f);
    }
}
