using System.Threading;
using System.Windows;
using PixelPainter.Foundation;
using PixelPainter.Render;
using PixelPainter.Resources;

namespace PixelPainter
{
    public partial class MainWindow : Window
    {
        private bool _running = true;

        public MainWindow()
        {
            InitializeComponent();

            //Do init job
            Mesh mesh = new Mesh("C:/Users/liiii/Documents/GitHub/PixelPainter/Pixel-Painter/Mesh/Box.obj");
            Singleton<RenderPipeline>.Instance.Init(RenderResult);
            Singleton<RenderPipeline>.Instance.AddRenderObject(new RenderObject(mesh));

            //Start Render Tick
            Thread renderJob = new Thread(RenderJob);
            renderJob.IsBackground = true;
            renderJob.Start();
        }

        private void RenderJob()
        {
            while (_running)
            {
                var pipeline = Singleton<RenderPipeline>.Instance;
                pipeline.RenderTick();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Title =
                        $"Griseo Render    delta:{pipeline.DeltaTime:0.0000}, FPS:{pipeline.FPS}, frame:{pipeline.CurrentFrame}";
                });
            }
        }
    }
}