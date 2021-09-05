using System.Collections.Generic;
using System.Diagnostics;
using Veldrid;
using Veldrid.StartupUtilities;

namespace Gerudo
{
    public abstract class Engine
    {
        public static Engine Instance = null;

        public Window Window { get; private set; }

        public RenderSystem RenderSystem { get; private set; }

        public AssetManagementSystem AssetManagementSystem { get; private set; }

        public Scene Scene { get; private set; }

        private List<ISubSystem> _subSystems = new List<ISubSystem>();

        private void Startup()
        {
            Instance = this;

            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 50,
                Y = 50,
                WindowWidth = 1920,
                WindowHeight = 1080,
                WindowTitle = "Gerudo",
                WindowInitialState = WindowState.Normal
            };
            Window = new Window(windowCI);

            InitializeSubSystems();

            foreach (var subSystem in _subSystems)
            {
                subSystem.Startup();
            }

            Scene = new Scene();
        }

        public void Run()
        {
            Startup();

            Initialize();

            Stopwatch sw = Stopwatch.StartNew();
            double previousElapsed = sw.Elapsed.TotalSeconds;

            while (Window.Exist)
            {
                double newElapsed = sw.Elapsed.TotalSeconds;
                float deltaTime = (float)(newElapsed - previousElapsed);

                InputSnapshot inputSnapshot = Window.NativeWindow.PumpEvents();
                Input.UpdateFrameInput(inputSnapshot, Window.NativeWindow);

                if (Window.Exist)
                {
                    previousElapsed = newElapsed;

                    Update(deltaTime);

                    RenderSystem.Render(Scene);
                }
            }

            Cleanup();

            Shutdown();
        }

        private void Shutdown()
        {
            for (int i = _subSystems.Count - 1; i >= 0; --i)
            {
                _subSystems[i].Shutdown();
            }
        }

        protected virtual void Initialize()
        {

        }

        protected virtual void Update(float deltaTime)
        {
        }

        protected virtual void Cleanup()
        {

        }

        private void InitializeSubSystems()
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions
            {
                Debug = true,
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true,
                SwapchainDepthFormat = PixelFormat.D24_UNorm_S8_UInt,
                ResourceBindingModel = ResourceBindingModel.Improved
            };
            GraphicsDevice device = VeldridStartup.CreateGraphicsDevice(Window.NativeWindow, options, GraphicsBackend.Vulkan);
            RenderSystem = new RenderSystem(device);

            AssetManagementSystem = new AssetManagementSystem();

            _subSystems.Add(RenderSystem);
            _subSystems.Add(AssetManagementSystem);
        }
    }
}
