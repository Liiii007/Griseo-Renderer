using System.Numerics;
using GriseoRenderer;
using GriseoRenderer.Render;

namespace PixelPainter.Render;

public class DirectLight
{
    public Vector4 Forward;
    public RealColor Color;
    public float Intensity;

    public DirectLight(Vector4 forward)
    {
        Forward = Vector4.Normalize(forward);
        Color = new RealColor(1);
        Intensity = 1f;
    }

    // public Vector4 Forward => Vector4.Normalize(Vector4.Transform(new Vector4(0, 0, 1, 0), Rotation));
}