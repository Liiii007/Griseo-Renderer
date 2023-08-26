using System.Numerics;
using GriseoRenderer;
using GriseoRenderer.Render;

namespace PixelPainter.Render;

public class DirectLight
{
    public Quaternion Rotation;
    public RealColor Color;
    public float Intensity;

    public DirectLight(Quaternion rotation)
    {
        Rotation = rotation;
        Color = new RealColor(1);
        Intensity = 1f;
    }

    public Vector4 Forward => Vector4.Normalize(Vector4.Transform(new Vector4(0, 0, 1, 0), Rotation));
}