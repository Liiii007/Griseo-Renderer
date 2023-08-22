using System.Numerics;
using System.Threading;
using System.Windows;
using GriseoRenderer.Foundation;
using GriseoRenderer.Render;
using GriseoRenderer.Resources;

namespace GriseoRenderer
{
    public partial class MainWindow : Window
    {
        private bool _running = true;

        public MainWindow()
        {
            InitializeComponent();

            var cameraManager = Singleton<CameraManager>.Instance;
            cameraManager.Init();
            Singleton<RenderPipeline>.Instance.Init(RenderResult, cameraManager.MainCamera);
            Singleton<InputManager>.Instance.OnW += () =>
            {
                cameraManager.MainCamera.AddPosition(new Vector3(0, 0, -0.1f));
            };
            Singleton<InputManager>.Instance.OnS += () =>
            {
                cameraManager.MainCamera.AddPosition(new Vector3(0, 0, 0.1f));
            };
            Singleton<InputManager>.Instance.OnA += () =>
            {
                cameraManager.MainCamera.AddPosition(new Vector3(-0.1f, 0, 0));
            };
            Singleton<InputManager>.Instance.OnD += () =>
            {
                cameraManager.MainCamera.AddPosition(new Vector3(0.1f, 0, 0));
            };
            Singleton<InputManager>.Instance.OnQ += () =>
            {
                cameraManager.MainCamera.AddPosition(new Vector3(0, -0.1f, 0));
            };
            Singleton<InputManager>.Instance.OnE += () =>
            {
                cameraManager.MainCamera.AddPosition(new Vector3(0, 0.1f, 0));
            };

            Mesh box = new Mesh(@"C:\Users\liiii\Documents\GitHub\Griseo-Render\GriseoRender\Box.obj");
            RenderObject obj = new RenderObject(box);
            Singleton<RenderPipeline>.Instance.AddRenderObject(obj);

            //Start Render Tick
            Thread renderJob = new Thread(Tick);
            renderJob.IsBackground = true;
            renderJob.Start();
        }

        private void Tick()
        {
            while (_running)
            {
                Singleton<InputManager>.Instance.Query();

                var pipeline = Singleton<RenderPipeline>.Instance;
                pipeline.RenderTick();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    double deltaTime = pipeline.DeltaTime * 1000;
                    Title = $"Griseo Renderer  DeltaMS:{deltaTime:0.00}, FPS:{pipeline.FPS}, Frame:{pipeline.CurrentFrame}";
                });
            }
        }
    }
}