using System;
using System.Drawing;
using System.Numerics;
using GriseoRender.Foundation;

namespace GriseoRender.Render;

public class Gizmos
{
    public static void DrawDot(Vector2 screenPos, int width)
    {
        var rt = Singleton<RenderPipeline>.Instance.CurrentRT;

        int x = (int)screenPos.X;
        int y = (int)screenPos.Y;

        for (int i = Math.Max(0, x - width); i < Math.Min(x + width, rt.Width); i++)
        {
            for (int j = Math.Max(0, y - width); j < Math.Min(y + width, rt.Height); j++)
            {
                rt[i, j] = ScreenColor.Red;
            }
        }
    }
}