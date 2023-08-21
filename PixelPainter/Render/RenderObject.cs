using PixelPainter.Resources;

namespace PixelPainter.Render;

public class RenderObject
{
    private Mesh _mesh;
    private Translation _translation;
    private VertexOut[] _verticesOut;

    public RenderObject(Mesh mesh)
    {
        _mesh = mesh;
        _verticesOut = new VertexOut[_mesh.Vertices.Length];
        _translation = new Translation();
    }
}