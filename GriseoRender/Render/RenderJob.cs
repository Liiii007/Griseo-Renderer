using GriseoRenderer.JobSystem;

namespace GriseoRenderer.Render;

public struct RenderJob : IJobFor
{
    public int xMin;
    public int xMax;
    public int yMin;
    public int yMax;

    public VertexOut[] fin;
    public RenderTarget colorRT;
    public DepthRenderTarget depthRT;
    public RenderPipeline pipeline;

    public void Execute(int index)
    {
        int width = xMax - xMin;
        int x = xMin + index % width;
        int y = yMin + index / width;

        var p0H = fin[0].PositionH;
        var p1H = fin[1].PositionH;
        var p2H = fin[2].PositionH;
        var (a, b, c) = RenderMath.GetBCCoord(p0H, p1H, p2H, new(x, y));
        var depth = p0H.Z * a + p1H.Z * b + p2H.Z * c;

        //Early-Z test: small->far
        if (depth <= depthRT[x, y])
        {
            return;
        }

        float minValue = 0;
        float maxValue = 1;

        if (minValue <= a && a <= maxValue && minValue <= b && b <= maxValue && minValue <= c && c <= maxValue)
        {
            var positionW = fin[0].PositionW * a + fin[1].PositionW * b + fin[2].PositionW * c;
            var normalW = fin[0].NormalW * a + fin[1].NormalW * b + fin[2].NormalW * c;
            var uv = fin[0].TexCoord * a + fin[1].TexCoord * b + fin[2].TexCoord * c;

            //Pixel shader
            //Write color rt
            colorRT[x, y] = pipeline.OpaqueLighting(positionW, normalW, uv);

            //Write depth rt
            depthRT[x, y] = depth;
        }
    }
}