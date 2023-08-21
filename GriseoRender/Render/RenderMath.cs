using System;
using System.Numerics;

namespace PixelPainter.Render;

public static class RenderMath
{
    private static float Min(float a, float b, float c)
    {
        return MathF.Min(a, MathF.Min(b, c));
    }

    private static float Max(float a, float b, float c)
    {
        return MathF.Max(a, MathF.Max(b, c));
    }

    public static (Vector2 min, Vector2 max) GetBoundBox(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2 min = new()
        {
            X = Min(p1.X, p2.X, p3.X),
            Y = Min(p1.Y, p2.Y, p3.Y),
        };
        Vector2 max = new()
        {
            X = Max(p1.X, p2.X, p3.X),
            Y = Max(p1.Y, p2.Y, p3.Y),
        };

        return (min, max);
    }


    public static (float a, float b, float c) GetBCCoord(Vector2 pA, Vector2 pB, Vector2 pC, Vector2 p)
    {
        float xa = pA.X;
        float xb = pB.X;
        float xc = pC.X;
        float ya = pA.Y;
        float yb = pB.Y;
        float yc = pC.Y;
        float x = p.X;
        float y = p.Y;

        float c = ((ya - yb) * x + (xb - xa) * y + xa * yb - xb * ya) /
                  ((ya - yb) * xc + (xb - xa) * yc + xa * yb - xb * ya);
        float b = ((ya - yc) * x + (xc - xa) * y + xa * yc - xc * ya) /
                  ((ya - yc) * xb + (xc - xa) * yb + xa * yc - xc * ya);
        float a = 1 - b - c;
        return (a, b, c);
    }

    public static Vector4 Multiply(Matrix4x4 mat, Vector4 vec)
    {
        //TODO:SIMD Optimize
        return new Vector4()
        {
            X = mat.M11 * vec.X + mat.M12 * vec.Y + mat.M13 * vec.Z + mat.M14 * vec.W,
            Y = mat.M21 * vec.X + mat.M22 * vec.Y + mat.M23 * vec.Z + mat.M24 * vec.W,
            Z = mat.M31 * vec.X + mat.M32 * vec.Y + mat.M33 * vec.Z + mat.M34 * vec.W,
            W = mat.M41 * vec.X + mat.M42 * vec.Y + mat.M43 * vec.Z + mat.M44 * vec.W,
        };
    }
}