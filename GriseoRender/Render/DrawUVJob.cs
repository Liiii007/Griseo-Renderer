using GriseoRenderer.JobSystem;

namespace GriseoRenderer.Render;

public struct DrawUVJob : IJobFor
{
    public RenderTarget target;

    public void Execute(int index)
    {
        int x = index % target.Width;
        int y = index / target.Width;

        float uvX = (float)x / target.Width;
        float uvY = (float)y / target.Height;

        ScreenColor color = new ScreenColor(uvX, uvY, 0);
        target[x, y] = color;
    }
}