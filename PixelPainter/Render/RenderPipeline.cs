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
    private List<RenderObject> _objects;
    private List<RenderTarget> _rts;
    private Stopwatch _stopwatch;

    public void Init(Image screen)
    {
        _stopwatch = Stopwatch.StartNew();
        _screen = screen;
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
        rt.Clear(ScreenColor.Red);

        RenderBackgroundUV();
        RenderSimpleTriangle();

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
        //TODO:Vertex
        //TODO:Fragment
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
            target[i] = color;
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
                    target[x + y * target.Width] = new ScreenColor()
                    {
                        R = a, G = b, B = c, A = 1
                    };
                }
            }
        }
    }
}