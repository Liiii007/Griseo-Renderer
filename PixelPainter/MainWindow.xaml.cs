using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using PixelPainter.Render;

namespace PixelPainter
{
    public partial class MainWindow : Window
    {
        private static RenderTarget _target;
        private static Image _screen;

        public static void RenderJob()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                var lastTime = stopwatch.ElapsedMilliseconds;

                _target.Clear();
                Render(_target);

                Console.WriteLine(stopwatch.ElapsedMilliseconds - lastTime);

                Thread.Sleep(100);
            }
        }

        public static void DrawJob()
        {
            while (true)
            {
                Application.Current.Dispatcher.Invoke(() => { _target.ApplyTo(_screen); });

                Thread.Sleep(100);
            }
        }

        public static Vector2 p1 = new(500, 100);
        public static Vector2 p2 = new(100, 600);
        public static Vector2 p3 = new(1000, 400);

        public MainWindow()
        {
            InitializeComponent();
            _screen = RenderResult;
            _target = new RenderTarget(1280, 720);

            Thread renderJob = new Thread(RenderJob);
            renderJob.IsBackground = true;
            renderJob.Start();

            Thread drawJob = new Thread(DrawJob);
            drawJob.IsBackground = true;
            drawJob.Start();
        }

        private static float Min(float a, float b, float c)
        {
            return MathF.Min(a, MathF.Min(b, c));
        }

        private static float Max(float a, float b, float c)
        {
            return MathF.Max(a, MathF.Max(b, c));
        }

        private static (Vector2 min, Vector2 max) GetBoundBox(Vector2 p1, Vector2 p2, Vector2 p3)
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

        private static (float a, float b, float c) GetBCCoord(Vector2 pA, Vector2 pB, Vector2 pC, Vector2 p)
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

        private static void Render(RenderTarget target)
        {
            //UV background
            for (int i = 0; i < target.Length; i++)
            {
                int x = i % target.Width;
                int y = i / target.Width;

                float uvX = (float)x / target.Width;
                float uvY = (float)y / target.Height;

                ScreenColor color = new ScreenColor();
                color.R = uvX;
                color.G = uvY;
                color.B = 0;
                color.A = 1f;
                target[i] = color;
            }

            var bound = GetBoundBox(p1, p2, p3);
            var xMin = (int)bound.min.X;
            var xMax = (int)bound.max.X;
            var yMin = (int)bound.min.Y;
            var yMax = (int)bound.max.Y;

            ScreenColor boundBoxColor = new ScreenColor()
            {
                R = 1,
                G = 1,
                B = 1,
                A = 1
            };

            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {
                    target[x + y * target.Width] = boundBoxColor;

                    var (a, b, c) = GetBCCoord(p1, p2, p3, new(x, y));

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
}