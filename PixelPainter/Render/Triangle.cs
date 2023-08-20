using System.Numerics;

namespace PixelPainter.Render;

public struct Triangle
{
    public Triangle(Vector3 vertex)
    {
        Vertex = vertex;
    }
    
    public Vector3 Vertex;
}