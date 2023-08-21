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
            Singleton<RenderPipeline>.Instance.Init(RenderResult);

            Mesh box = new Mesh(@"C:\Users\liiii\Documents\GitHub\Griseo-Render\GriseoRender\Box.obj");
            RenderObject obj = new RenderObject(box);
            Singleton<RenderPipeline>.Instance.AddRenderObject(obj);

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
                        $"Griseo Render    DeltaMS:{pipeline.DeltaTime * 1000:0.00}, FPS:{pipeline.FPS}, Frame:{pipeline.CurrentFrame}";
                });
            }
        }
    }
}