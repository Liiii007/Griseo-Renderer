using System.Numerics;

namespace GriseoRenderer.Render;

public struct Vertex
{
    public Vector4 Position;
    public Vector2 TexCoord;
    public Vector4 Normal;
}

public struct Face
{
    public int a;
    public int b;
    public int c;
}

public struct VertexOut
{
    public Vector4 Position;
    public Vector4 Normal;
    public Vector2 TexCoord;
}