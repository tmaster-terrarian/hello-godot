using Godot;

namespace NewGameProject.Api;

public static class Draw3D
{
    private static readonly StandardMaterial3D StandardMaterial = new StandardMaterial3D()
    {
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha
    };

    public static MeshInstance3D Grid(Vector3 corner, int width, int height, Color? color = null, Material? material = null)
    {
        var parent = new MeshInstance3D();
        for (int x = 0; x <= width; x++)
        {
            Vector3 pos1 = corner + Vector3.Right * x;
            Vector3 pos2 = pos1 + Vector3.Up * height;
            var line = Line(pos1, pos2, color, material);
            parent.AddChild(line);
        }
        for (int y = 0; y <= height; y++)
        {
            Vector3 pos1 = corner + Vector3.Up * y;
            Vector3 pos2 = pos1 + Vector3.Right * width;
            var line = Line(pos1, pos2, color, material);
            parent.AddChild(line);
        }

        return parent;
    }

    public static MeshInstance3D Line(Vector3 pos1, Vector3 pos2, Color? color = null, Material? material = null)
    {
        var meshInstance = new MeshInstance3D();
        var immediateMesh = new ImmediateMesh();
        var mat = material ?? StandardMaterial;

        meshInstance.Mesh = immediateMesh;
        meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

        immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, mat);
        immediateMesh.SurfaceSetColor(color ?? Colors.White);
        immediateMesh.SurfaceAddVertex(pos1);
        immediateMesh.SurfaceSetColor(color ?? Colors.White);
        immediateMesh.SurfaceAddVertex(pos2);
        immediateMesh.SurfaceEnd();

        (Engine.GetMainLoop() as SceneTree)?.Root.AddChild(meshInstance);

        return meshInstance;
    }

    public static MeshInstance3D Point(Vector3 pos, float radius = 0.05f, Color? color = null)
    {
        var meshInstance = new MeshInstance3D();
        var sphereMesh = new SphereMesh();
        var material = new StandardMaterial3D();

        meshInstance.Mesh = sphereMesh;
        meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        meshInstance.Position = pos;

        sphereMesh.Radius = radius;
        sphereMesh.Height = radius * 2f;
        sphereMesh.Material = material;

        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        material.AlbedoColor = color ?? Colors.WhiteSmoke;

        (Engine.GetMainLoop() as SceneTree)?.Root.AddChild(meshInstance);

        return meshInstance;
    }

}
