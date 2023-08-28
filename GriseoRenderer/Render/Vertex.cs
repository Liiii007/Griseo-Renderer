using System.Numerics;

namespace GriseoRenderer.Render;

public struct Vertex
{
    public Vector4 PositionW;
    public Vector2 TexCoord;
    public Vector4 NormalW;
}

public struct Face
{
    public int a;
    public int b;
    public int c;
}

public struct VertexOut
{
    public Vector4 PositionH;
    public Vector4 PositionW;
    public Vector4 PositionLightH;
    public Vector4 NormalW;
    public Vector2 TexCoord;
}

public struct TriangleIn
{
    public VertexOut v1;
    public VertexOut v2;
    public VertexOut v3;
}