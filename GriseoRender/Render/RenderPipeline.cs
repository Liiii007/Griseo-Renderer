#pragma warning disable CS8618
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using PixelPainter.Render;

namespace GriseoRenderer.Render;

public class RenderPipeline
{
    private Image _screen;
    private Camera _camera;
    private List<RenderObject> _objects;
    private List<DirectLight> _lights;
    private List<RenderTarget> _rts;
    private List<DepthRenderTarget> _drts;
    private Texture _mainTexSampler;
    private Texture _normalMapSampler;
    private Stopwatch _stopwatch;

    public void Init(Image screen, Camera camera)
    {
        _stopwatch = Stopwatch.StartNew();
        _screen = screen;
        _camera = camera;

        _objects = new List<RenderObject>();
        _lights = new List<DirectLight>();
        _rts = new List<RenderTarget>()
        {
            new(1280, 720),
            new(1280, 720)
        };
        _drts = new List<DepthRenderTarget>()
        {
            new(1280, 720),
            new(1280, 720)
        };
    }

    public void AddRenderObject(RenderObject renderObject)
    {
        _objects.Add(renderObject);
    }

    public void AddDirectLight(DirectLight light)
    {
        _lights.Add(light);
    }

    public RenderTarget CurrentRT => _rts[CurrentFrame % _rts.Count];
    public DepthRenderTarget CurrentDepthRT => _drts[CurrentFrame % _drts.Count];
    public int CurrentFrame { get; private set; }
    public double DeltaTime { get; private set; }
    public int FPS => (int)(1 / (DeltaTime >= 0 ? DeltaTime : 2));

    public void RenderTick()
    {
        double startTime = _stopwatch.Elapsed.TotalSeconds;

        var colorRT = CurrentRT;
        var depthRT = CurrentDepthRT;
        colorRT.Clear(ScreenColor.Black);
        depthRT.Clear(-1);

        // RenderLine();
        // RenderBackgroundUV();

        foreach (var obj in _objects)
        {
            RenderObject(obj, colorRT, depthRT);
        }

        SwapAndShow();

        double endTime = _stopwatch.Elapsed.TotalSeconds;
        DeltaTime = (endTime - startTime);
    }

    public void RenderObject(RenderObject obj, RenderTarget colorRT, DepthRenderTarget depthRT)
    {
        //Vertex
        Matrix4x4 m = obj.M;
        Matrix4x4 mvp = _camera.VP * m;

        for (int i = 0; i < obj.Mesh.Vertices.Length; i++)
        {
            Vertex vin = obj.Mesh.Vertices[i];
            VertexOut vout = new VertexOut();
            vout.PositionH = RenderMath.Multiply(mvp, vin.PositionW);
            vout.PositionW = RenderMath.Multiply(m, vin.PositionW);
            vout.NormalW = RenderMath.Multiply(m, vin.NormalW);
            vout.TexCoord = vin.TexCoord;

            obj.VerticesOut[i] = vout;
        }

        //Rasterization
        var mViewport = Matrix4x4.Identity;
        mViewport[0, 0] = colorRT.Width / 2f;
        mViewport[0, 3] = colorRT.Width / 2f;
        mViewport[1, 1] = colorRT.Height / 2f;
        mViewport[1, 3] = colorRT.Height / 2f;

        for (int i = 0; i < obj.VerticesOut.Length; i++)
        {
            VertexOut vout = obj.VerticesOut[i];

            //Viewport transform(0,0,width,height)
            vout.PositionH /= vout.PositionH.W;
            vout.PositionH = RenderMath.Multiply(mViewport, vout.PositionH);
            obj.VerticesOut[i] = vout;
        }

        //Fragment
        var indices = obj.Mesh.Indices;

        var fin = new VertexOut[3];

        for (int i = 0; i < indices.Length; i++)
        {
            var faceIndex = indices[i];

            fin[0] = obj.VerticesOut[faceIndex.a];
            fin[1] = obj.VerticesOut[faceIndex.b];
            fin[2] = obj.VerticesOut[faceIndex.c];

            Vector4 p1 = fin[0].PositionW;
            Vector4 p2 = fin[1].PositionW;
            Vector4 p3 = fin[2].PositionW;

            if (p1.Z < -1 || p2.Z < -1 || p3.Z < -1)
            {
                continue;
            }

            //Back culling
            Vector4 v1 = p2 - p1;
            Vector4 v2 = p3 - p2;

            Vector4 normalFace = RenderMath.Cross(v1, v2);

            if (RenderMath.Dot(normalFace, _camera.Forward) > 0)
            {
                continue;
            }

            _mainTexSampler = obj.MainTex;
            _normalMapSampler = obj.MainTex;
            Rasterlation(fin, colorRT, depthRT);
        }

        //Debug:Draw dot in vertices
        for (int i = 0; i < obj.VerticesOut.Length; i++)
        {
            var pos = obj.VerticesOut[i].PositionH;
            var posInScreen = new Vector2(pos.X, pos.Y);
            Gizmos.DrawDot(posInScreen, 2);
        }
    }

    public void SwapAndShow()
    {
        Application.Current.Dispatcher.Invoke(() => { CurrentRT.ApplyTo(_screen); });
        CurrentFrame++;
    }

    private void RenderBackgroundUV()
    {
        var target = CurrentRT;
        //UV background
        for (int i = 0; i < target.Length; i++)
        {
            int x = i % target.Width;
            int y = i / target.Width;

            float uvX = (float)x / target.Width;
            float uvY = (float)y / target.Height;

            ScreenColor color = new ScreenColor(uvX, uvY, 0);
            target[x, y] = color;
        }
    }

    private void RenderLine()
    {
        var target = CurrentRT;
        for (int i = 0; i < target.Height; i++)
        {
            target[target.Width / 2, i] = ScreenColor.Red;
        }

        for (int i = 0; i < target.Width; i++)
        {
            target[i, target.Height / 2] = ScreenColor.Red;
        }
    }

    private void Rasterlation(VertexOut[] fin, RenderTarget colorRT, DepthRenderTarget depthRT)
    {
        var p0H = fin[0].PositionH;
        var p1H = fin[1].PositionH;
        var p2H = fin[2].PositionH;

        var p0W = fin[0].PositionW;
        var p1W = fin[1].PositionW;
        var p2W = fin[2].PositionW;

        var bound = RenderMath.GetBoundBox2D(p0H, p1H, p2H);
        var xMin = (int)(bound.min.X + 0.5f);
        var xMax = (int)(bound.max.X + 0.5f);
        var yMin = (int)(bound.min.Y + 0.5f);
        var yMax = (int)(bound.max.Y + 0.5f);

        xMin = Math.Max(0, xMin);
        xMax = Math.Min(colorRT.Width - 1, xMax);
        yMin = Math.Max(0, yMin);
        yMax = Math.Min(colorRT.Height - 1, yMax);

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                var (a, b, c) = RenderMath.GetBCCoord(p0H, p1H, p2H, new(x, y));
                var depth = p0H.Z * a + p1H.Z * b + p2H.Z * c;

                //Early-Z test: small->far
                if (depth <= depthRT[x, y])
                {
                    continue;
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
                    colorRT[x, y] = OpaqueLighting(positionW, normalW, uv);

                    //Write depth rt
                    depthRT[x, y] = depth;
                }
            }
        }
    }

    private ScreenColor OpaqueLighting(Vector4 positionW, Vector4 normalW, Vector2 uv)
    {
        var texColor = new RealColor(1);
        if (_mainTexSampler != null)
        {
            texColor = _mainTexSampler[(int)(uv.X * _mainTexSampler.Width), (int)(uv.Y * _mainTexSampler.Height)];
        }

        var ambient = new RealColor(0.1f, 0.1f, 0.1f);
        var diffuse = new RealColor();
        var specular = new RealColor();

        foreach (var light in _lights)
        {
            var diffuseColor = light.Color * light.Intensity * RenderMath.Dot(light.Forward, -normalW);
            diffuse += diffuseColor.Step0;

            var viewDir = Vector4.Normalize(positionW - _camera.Position);
            var half = -Vector4.Normalize(viewDir + light.Forward);
            var specularStrength = MathF.Pow(RenderMath.Clamp01(RenderMath.Dot(half, normalW)), 1024);
            var specularColor = light.Color * light.Intensity * specularStrength;
            specular += specularColor.Step0;
        }

        var rColor = ambient + diffuse + specular;
        rColor *= texColor;
        var screenColor = rColor.AsScreenColor;
        screenColor.A = 1f;
        return screenColor;
    }
}