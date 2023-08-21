#pragma warning disable CS8618
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace PixelPainter.Render;

public class RenderPipeline
{
    private Image _screen;
    private Camera _camera;
    private List<RenderObject> _objects;
    private List<RenderTarget> _rts;
    private Stopwatch _stopwatch;

    public void Init(Image screen)
    {
        _stopwatch = Stopwatch.StartNew();
        _screen = screen;
        _camera = new Camera(new Vector3(0, 1.5f, 2), Quaternion.CreateFromYawPitchRoll(0, 0, 0));
        _objects = new List<RenderObject>();
        _rts = new List<RenderTarget>()
        {
            new RenderTarget(1280, 720),
            new RenderTarget(1280, 720)
        };
    }

    public void AddRenderObject(RenderObject renderObject)
    {
        _objects.Add(renderObject);
    }

    public RenderTarget CurrentRT => _rts[CurrentFrame % _rts.Count];
    public int CurrentFrame { get; private set; }
    public double DeltaTime { get; private set; }
    public int FPS => (int)(1 / (DeltaTime >= 0 ? DeltaTime : 2));

    public void RenderTick()
    {
        double startTime = _stopwatch.Elapsed.TotalSeconds;

        var rt = CurrentRT;
        rt.Clear(ScreenColor.Black);

        foreach (var obj in _objects)
        {
            RenderObject(obj, rt);
        }

        SwapAndShow();

        double endTime = _stopwatch.Elapsed.TotalSeconds;
        DeltaTime = (endTime - startTime);
    }

    public void RenderObject(RenderObject obj, RenderTarget rt)
    {
        //Vertex
        Matrix4x4 mvp = _camera.VP * obj.M;

        for (int i = 0; i < obj.Mesh.Vertices.Length; i++)
        {
            Vertex vin = obj.Mesh.Vertices[i];
            VertexOut vout = new VertexOut();
            vout.Position = RenderMath.Multiply(mvp, vin.Position);
            vout.Normal = RenderMath.Multiply(mvp, vin.Normal);
            vout.TexCoord = vin.TexCoord;

            obj.VerticesOut[i] = vout;
        }

        //Rasterization
        var mViewport = Matrix4x4.Identity;
        mViewport[0, 0] = rt.Width / 2f;
        mViewport[0, 3] = rt.Width / 2f;
        mViewport[1, 1] = rt.Height / 2f;
        mViewport[1, 3] = rt.Height / 2f;

        for (int i = 0; i < obj.VerticesOut.Length; i++)
        {
            VertexOut vout = obj.VerticesOut[i];
            vout.Position /= vout.Position.W;
            vout.Position = RenderMath.Multiply(mViewport, vout.Position);
            obj.VerticesOut[i] = vout;
        }

        //Fragment
        var indices = obj.Mesh.Indices;
        for (int i = 0; i < indices.Length; i++)
        {
            var faceIndex = indices[i];

            Vector4 p1 = obj.VerticesOut[faceIndex.a].Position;
            Vector4 p2 = obj.VerticesOut[faceIndex.b].Position;
            Vector4 p3 = obj.VerticesOut[faceIndex.c].Position;

            DrawTriangle(p1, p2, p3, rt);
        }

        //Debug:Draw dot in vertices
        for (int i = 0; i < obj.VerticesOut.Length; i++)
        {
            var pos = obj.VerticesOut[i].Position;
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

    private void DrawTriangle(Vector4 p1, Vector4 p2, Vector4 p3, RenderTarget colorRT)
    {
        DrawTriangle(new Vector2(p1.X, p1.Y), new Vector2(p2.X, p2.Y), new Vector2(p3.X, p3.Y), colorRT);
    }

    private void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, RenderTarget colorRT)
    {
        var bound = RenderMath.GetBoundBox(p1, p2, p3);
        var xMin = (int)bound.min.X;
        var xMax = (int)bound.max.X;
        var yMin = (int)bound.min.Y;
        var yMax = (int)bound.max.Y;

        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                var (a, b, c) = RenderMath.GetBCCoord(p1, p2, p3, new(x, y));

                if (0 < a && a < 1 && 0 < b && b < 1 && 0 < c && c < 1)
                {
                    colorRT[x, y] = ScreenColor.White;
                }
            }
        }
    }

    private void RenderSimpleTriangle()
    {
        var target = CurrentRT;

        Vector2 p1 = new(500, 100);
        Vector2 p2 = new(100, 600);
        Vector2 p3 = new(1000, 400);

        var bound = RenderMath.GetBoundBox(p1, p2, p3);
        var xMin = (int)bound.min.X;
        var xMax = (int)bound.max.X;
        var yMin = (int)bound.min.Y;
        var yMax = (int)bound.max.Y;


        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                var (a, b, c) = RenderMath.GetBCCoord(p1, p2, p3, new(x, y));

                if (0 < a && a < 1 && 0 < b && b < 1 && 0 < c && c < 1)
                {
                    target[x, y] = new ScreenColor()
                    {
                        R = a, G = b, B = c, A = 1
                    };
                }
            }
        }
    }
}