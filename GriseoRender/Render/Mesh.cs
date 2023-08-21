using System.Numerics;
using PixelPainter.Render;

namespace PixelPainter.Resources;

public class Mesh
{
    public readonly Vertex[] Vertices;
    public readonly Face[] Indices;

    public Mesh(string path)
    {
        var scene = Import(path);
        var mesh = scene.Meshes[0];

        //Copy vertices
        {
            Vertices = new Vertex[mesh.VertexCount];
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                var pos = mesh.Vertices[i];
                var normal = mesh.Normals[i];
                var uv = mesh.TextureCoordinateChannels[0][i];

                Vertices[i] = new Vertex()
                {
                    Position = new Vector4(pos.X, pos.Y, pos.Z, 1),
                    Normal = new Vector4(normal.X, normal.Y, normal.Z, 0),
                    TexCoord = new Vector2(uv.X, uv.Y)
                };
            }
        }

        //Copy indices
        {
            var faces = mesh.Faces;
            Indices = new Face[faces.Count];
            for (int i = 0; i < faces.Count; i++)
            {
                Indices[i].a = faces[i].Indices[0];
                Indices[i].b = faces[i].Indices[1];
                Indices[i].c = faces[i].Indices[2];
            }
        }
    }

    public static Assimp.Scene Import(string path)
    {
        var importer = new Assimp.AssimpContext();
        importer.SetConfig(new Assimp.Configs.NormalSmoothingAngleConfig(66.0f));
        var scene = importer.ImportFile(path, Assimp.PostProcessPreset.TargetRealTimeMaximumQuality);
        return scene;
    }
}