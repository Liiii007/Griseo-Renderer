#pragma warning disable CS8618
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using GriseoRenderer.Foundation;
using GriseoRenderer.JobSystem;
using PixelPainter.Render;

namespace GriseoRenderer.Render;

public class RenderPipeline
{
    private Image _screen;
    private Camera _mainCamera;
    private Camera _shadowCamera;
    private List<RenderObject> _objects;
    private List<DirectLight> _lights;
    private List<RenderTarget> _rts;
    private List<DepthRenderTarget> _depthRTs;
    private List<DepthRenderTarget> _shadowMapRTs;
    private Texture _mainTexSampler;
    private Texture _normalMapSampler;
    private Stopwatch _stopwatch;

    public void Init(Image screen, Camera camera)
    {
        _stopwatch = Stopwatch.StartNew();
        _screen = screen;
        _mainCamera = camera;
        _shadowCamera = new Camera(Vector3.Zero, Quaternion.Zero)
        {
            IsOrith = true,
            f = 20,
            Height = 1024,
            Width = 1024
        };

        _objects = new List<RenderObject>();
        _lights = new List<DirectLight>();
        _rts = new List<RenderTarget>()
        {
            new(1280, 720),
            new(1280, 720)
        };
        _depthRTs = new List<DepthRenderTarget>()
        {
            new(1280, 720),
            new(1280, 720)
        };
        _shadowMapRTs = new List<DepthRenderTarget>()
        {
            new(1024, 1024),
            new(1024, 1024)
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
    public DepthRenderTarget CurrentDepthRT => _depthRTs[CurrentFrame % _depthRTs.Count];
    public DepthRenderTarget CurrentShadowMapRT => _shadowMapRTs[CurrentFrame % _depthRTs.Count];
    public int CurrentFrame { get; private set; }
    public double DeltaTime { get; private set; }
    public int FPS => (int)(1 / (DeltaTime >= 0 ? DeltaTime : 2));

    public void RenderTick()
    {
        double startTime = _stopwatch.Elapsed.TotalSeconds;

        var colorRT = CurrentRT;
        var depthRT = CurrentDepthRT;
        colorRT.Clear(ScreenColor.Black);
        depthRT.Clear(1);
        CurrentShadowMapRT.Clear(1);

        // RenderLine();
        // RenderUvJob();
        _shadowCamera.Position = _lights[0].Forward * -10;
        _shadowCamera.Rotation = _lights[0].Rotation;

        //Pass1:ShadowMap Render
        foreach (var obj in _objects)
        {
            RenderObject(obj, _shadowCamera, null, CurrentShadowMapRT, true);
        }

        //Pass2:Object Render
        foreach (var obj in _objects)
        {
            RenderObject(obj, _mainCamera, colorRT, depthRT);
        }

        SwapAndShow();

        double endTime = _stopwatch.Elapsed.TotalSeconds;
        DeltaTime = (endTime - startTime);
    }

    public void RenderObject(RenderObject obj, Camera camera, RenderTarget colorRT, DepthRenderTarget depthRT,
        bool depthOnly = false)
    {
        //Vertex Shading
        Matrix4x4 m = obj.M;
        Matrix4x4 mvp = camera.VP * m;
        Matrix4x4 lightMVP = _shadowCamera.VP * m;

        for (int i = 0; i < obj.Mesh.Vertices.Length; i++)
        {
            Vertex vin = obj.Mesh.Vertices[i];

            VertexOut vout = new VertexOut
            {
                PositionH = RenderMath.Multiply(mvp, vin.PositionW),
                PositionW = RenderMath.Multiply(m, vin.PositionW),
                PositionLightH = RenderMath.Multiply(lightMVP, vin.PositionW),
                NormalW = RenderMath.Multiply(m, vin.NormalW),
                TexCoord = vin.TexCoord
            };

            obj.VerticesOut[i] = vout;
        }

        for (int i = 0; i < obj.VerticesOut.Length; i++)
        {
            VertexOut vout = obj.VerticesOut[i];
            if (!camera.IsOrith)
            {
                //To NDC
                vout.PositionH.X /= vout.PositionH.W;
                vout.PositionH.Y /= vout.PositionH.W;
                vout.PositionH.Z /= vout.PositionH.W;

                //To Linear Depth
                vout.PositionH.Z = (vout.PositionH.W - camera.n) / camera.f;
            }
            else
            {
                vout.PositionH.Z = (RenderMath.Dot(camera.Forward, vout.PositionW - camera.Position) - camera.n) /
                                   camera.f;
            }

            //Viewport transform
            vout.PositionH.X = vout.PositionH.X * depthRT.Width / 2 + depthRT.Width / 2f;
            vout.PositionH.Y = vout.PositionH.Y * depthRT.Height / 2 + depthRT.Height / 2f;

            obj.VerticesOut[i] = vout;
        }

        //Handle light position
        for (int i = 0; i < obj.VerticesOut.Length; i++)
        {
            VertexOut vout = obj.VerticesOut[i];
            vout.PositionLightH.X =
                vout.PositionLightH.X * CurrentShadowMapRT.Width / 2 + CurrentShadowMapRT.Width / 2f;
            vout.PositionLightH.Y =
                vout.PositionLightH.Y * CurrentShadowMapRT.Height / 2 + CurrentShadowMapRT.Height / 2f;

            //To Linear Depth
            vout.PositionLightH.Z = (RenderMath.Dot(_shadowCamera.Forward, vout.PositionW - _shadowCamera.Position) - _shadowCamera.n) /
                               _shadowCamera.f;

            obj.VerticesOut[i] = vout;
        }

        //Fragment Shading
        foreach (var faceIndexer in obj.Mesh.Indices)
        {
            //Triangle assembly
            var fin = new TriangleIn
            {
                v1 = obj.VerticesOut[faceIndexer.a],
                v2 = obj.VerticesOut[faceIndexer.b],
                v3 = obj.VerticesOut[faceIndexer.c]
            };

            Vector4 p1 = fin.v1.PositionH;
            Vector4 p2 = fin.v2.PositionH;
            Vector4 p3 = fin.v3.PositionH;

            //Back culling
            Vector4 v1 = p2 - p1;
            Vector4 v2 = p3 - p2;

            Vector4 normalFace = RenderMath.Cross(v1, v2);

            if (RenderMath.Dot(normalFace, camera.Forward) >= 0)
            {
                continue;
            }

            _mainTexSampler = obj.MainTex;
            _normalMapSampler = obj.MainTex;
            Rasterization(fin, colorRT, depthRT, depthOnly);
        }

        //Debug:Draw dot in vertices
        for (int i = 0; i < obj.VerticesOut.Length; i++)
        {
            var pos = obj.VerticesOut[i].PositionH;
            var posInScreen = new Vector2(pos.X, pos.Y);
            Gizmos.DrawDot(posInScreen, 2);
        }
    }

    private void Rasterization(TriangleIn fin, RenderTarget colorRT, DepthRenderTarget depthRT, bool depthOnly = false)
    {
        var p0H = fin.v1.PositionH;
        var p1H = fin.v2.PositionH;
        var p2H = fin.v3.PositionH;

        var bound = RenderMath.GetBoundBox2D(p0H, p1H, p2H);
        var xMin = (int)(bound.min.X + 0.5f);
        var xMax = (int)(bound.max.X + 0.5f);
        var yMin = (int)(bound.min.Y + 0.5f);
        var yMax = (int)(bound.max.Y + 0.5f);

        xMin = Math.Max(0, xMin);
        xMax = Math.Min(depthRT.Width - 1, xMax);
        yMin = Math.Max(0, yMin);
        yMax = Math.Min(depthRT.Height - 1, yMax);

        int width = xMax - xMin + 1;
        int height = yMax - yMin + 1;

        if (width <= 0 || height <= 0)
        {
            return;
        }

        var job = new RenderJob
        {
            xMin = xMin,
            yMin = yMin,
            width = width,
            fin = fin,
            colorRT = colorRT,
            depthRT = depthRT,
            depthOnly = depthOnly,
            pipeline = this
        };

        var handle = JobScheduler.Schedule(job, 0, width * height, 128);
        handle.Complete();
    }

    public ScreenColor OpaqueLighting(Vector4 positionW, Vector4 normalW, Vector2 uv, float shadowRate)
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

            var viewDir = Vector4.Normalize(positionW - _mainCamera.Position);
            var half = -Vector4.Normalize(viewDir + light.Forward);
            var specularStrength = MathF.Pow(RenderMath.Clamp01(RenderMath.Dot(half, normalW)), 256);
            var specularColor = light.Color * light.Intensity * specularStrength;
            specular += specularColor.Step0;
        }

        var rColor = ambient + shadowRate * (diffuse + specular);
        rColor *= texColor;
        var screenColor = rColor.AsScreenColor;
        screenColor.A = 1f;
        return screenColor;
    }

    public float SampleShadowMap(Vector4 positionLightH)
    {
        var shadowMap = CurrentShadowMapRT;
        int x = (int)(positionLightH.X + 0.5f);
        int y = (int)(positionLightH.Y + 0.5f);

        x = Math.Max(0, Math.Min(x, shadowMap.Width - 1));
        y = Math.Max(0, Math.Min(y, shadowMap.Height - 1));

        float currentDepth = positionLightH.Z;
        float mapDepth = shadowMap[x, y];
        float bias = 0.001f;

        if (currentDepth <= mapDepth + bias)
        {
            return 1;
        }
        else
        {
            return 0;
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

    private void RenderUvJob()
    {
        var target = CurrentRT;
        var job = new DrawUVJob()
        {
            target = target
        };
        var handle = JobScheduler.Schedule(job, 0, target.Length, 2048);
        handle.Complete();
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
}