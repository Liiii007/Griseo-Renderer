using System.Numerics;
using System.Threading;
using System.Windows;
using GriseoRenderer.Foundation;
using GriseoRenderer.Render;
using GriseoRenderer.Resources;
using PixelPainter.Render;

namespace GriseoRenderer
{
    public partial class MainWindow : Window
    {
        private bool _running = true;
        private DirectLight light;

        public MainWindow()
        {
            InitializeComponent();

            //Init
            var cameraManager = Singleton<CameraManager>.Instance;
            cameraManager.Init();
            Singleton<RenderPipeline>.Instance.Init(RenderResult, cameraManager.MainCamera);

            //TODO:Move to json config
            //Add objects and lights
            Mesh box = new Mesh(@"C:\Users\liiii\Documents\GitHub\Griseo-Render\GriseoRender\Box.obj");
            Texture mainTex = new Texture(@"C:\Users\liiii\Documents\GitHub\Griseo-Render\GriseoRender\MainTex.png");
            RenderObject obj = new RenderObject(box, mainTex);
            Singleton<RenderPipeline>.Instance.AddRenderObject(obj);
            light = new DirectLight(RenderMath.EulerToQuaternion(0, 0, 0));
            Singleton<RenderPipeline>.Instance.AddDirectLight(light);

            //Start tick thread
            Thread renderJob = new Thread(Tick);
            renderJob.IsBackground = true;
            renderJob.Start();
        }

        private void Tick()
        {
            while (_running)
            {
                var pipeline = Singleton<RenderPipeline>.Instance;
                Singleton<InputManager>.Instance.Query();

                // light.Rotation = RenderMath.EulerToQuaternion(pipeline.CurrentFrame * 0.016f * 100f, -30f, 0);

                pipeline.RenderTick();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    double deltaTime = pipeline.DeltaTime * 1000;
                    Title =
                        $"Griseo Renderer  DeltaMS:{deltaTime:0.00}, FPS:{pipeline.FPS}, Frame:{pipeline.CurrentFrame}";
                });
            }
        }
    }
}