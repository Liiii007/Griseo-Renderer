using System.Collections;
using GriseoRenderer.JobSystem;

namespace GriseoRenderer.Render;

public struct DrawUVJob : IYieldJobFor
{
    public RenderTarget target;

    public IEnumerator Execute(int start, int end)
    {
        for (int index = start; index < end; index++)
        {
            int x = index % target.Width;
            int y = index / target.Width;

            float uvX = (float)x / target.Width;
            float uvY = (float)y / target.Height;

            ScreenColor color = new ScreenColor(uvX, uvY, 0);
            target[x, y] = color;
        }
        
        yield break;
    }
}