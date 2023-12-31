﻿using System.Collections;
using GriseoRenderer.JobSystem;

namespace GriseoRenderer.Render;

public struct RenderJob : IYieldJobFor
{
    public int xMin;
    public int yMin;
    public int width;

    public TriangleIn fin;
    public RenderTarget colorRT;
    public DepthRenderTarget depthRT;
    public RenderPipeline pipeline;
    public bool depthOnly;

    public IEnumerator Execute(int startIndex, int endIndex)
    {
        for (int index = startIndex; index < endIndex; index++)
        {
            int x = xMin + index % (width);
            int y = yMin + index / (width);

            var p0H = fin.v1.PositionH;
            var p1H = fin.v2.PositionH;
            var p2H = fin.v3.PositionH;
            var (a, b, c) = RenderMath.GetBCCoord(p0H, p1H, p2H, new(x, y));
            var depth = p0H.Z * a + p1H.Z * b + p2H.Z * c;

            //Early-Z test: small->far
            if (depth >= depthRT[x, y])
            {
                continue;
            }

            float minValue = 0;
            float maxValue = 1;

            if (minValue <= a && a <= maxValue && minValue <= b && b <= maxValue && minValue <= c && c <= maxValue)
            {
                if (!depthOnly)
                {
                    var positionW = fin.v1.PositionW * a + fin.v2.PositionW * b + fin.v3.PositionW * c;
                    var normalW = fin.v1.NormalW * a + fin.v2.NormalW * b + fin.v3.NormalW * c;
                    var uv = fin.v1.TexCoord * a + fin.v2.TexCoord * b + fin.v3.TexCoord * c;
                    var positionLightH = fin.v1.PositionLightH * a + fin.v2.PositionLightH * b +
                                         fin.v3.PositionLightH * c;
                    var shadowRate = pipeline.SampleShadowMap(positionLightH);

                    //Pixel shader
                    //Write color rt
                    colorRT[x, y] = pipeline.OpaqueLighting(positionW, normalW, uv, shadowRate);
                }

                //Write depth rt
                depthRT[x, y] = depth;
            }
        }

        yield break;
    }
}