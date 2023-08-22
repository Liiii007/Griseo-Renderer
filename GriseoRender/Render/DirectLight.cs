using System.Numerics;
using GriseoRenderer;

namespace PixelPainter.Render;

public class DirectLight
{
    public Translation Translation;

    public Vector4 Forward => Vector4.Transform(new Vector3(0, 0, 1), Translation.Rotation);
}