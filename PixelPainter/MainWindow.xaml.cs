using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using PixelPainter.Render;

namespace PixelPainter
{
    public partial class MainWindow : Window
    {
        private static RenderTarget _target;
        private static Image        _screen;

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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _target.ApplyTo(_screen);
                });
                
                Thread.Sleep(100);
            }
        }

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

        private static void Render(RenderTarget target)
        {
            for (int i = 0; i < target.Length; i++)
            {
                int x = i % target.Width;
                int y = i / target.Width;

                float uvX = (float)x / target.Width;
                float uvY = (float)y / target.Height;

                MColor color = new MColor();
                color.R = uvX;
                color.G = uvY;
                color.B = 0;
                color.A = 1f;
                target.SetColor(color, i);
            }

            float[] value = new float[720];

            for (int i = 0; i < 700; i++)
            {
                value[i] = target.GetColor(0, i).G;
            }
        }
    }
}