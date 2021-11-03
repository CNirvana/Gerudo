using System.Diagnostics;
using Veldrid;
using Veldrid.StartupUtilities;

namespace Gerudo
{
    public abstract class Engine
    {
        public static Engine Instance = null;

        public Window Window { get; private set; }

        public RenderManager RenderManager { get; private set; }

        public AssetManager AssetManager { get; private set; }

        public Scene Scene { get; private set; }

        private bool _isRunning;

        private bool Startup()
        {
            Instance = this;

            Logger.Initialize();

            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 50,
                Y = 50,
                WindowWidth = 1280,
                WindowHeight = 720,
                WindowTitle = "Gerudo",
                WindowInitialState = WindowState.Normal
            };
            Window = new Window(windowCI);

            AssetManager = new AssetManager();
            if (!AssetManager.Startup())
            {
                Logger.LogError("Failed to initialize asset manager!");
            }

            RenderManager = new RenderManager();
            if (!RenderManager.Startup(Window))
            {
                Logger.LogError("Failed to initialize render manager!");
                return false;
            }

            Scene = new Scene();

            return true;
        }

        public void Run()
        {
            if (!Startup())
            {
                Logger.LogError("Engine could not initialize successfully!");
                Shutdown();
                return;
            }

            Initialize();

            Stopwatch sw = Stopwatch.StartNew();
            double previousElapsed = sw.Elapsed.TotalSeconds;

            _isRunning = true;
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

                    RenderManager.ProcessInput(deltaTime, inputSnapshot);
                    OnGUI();
                    RenderManager.Render(Scene);

                    if (_isRunning == false)
                    {
                        Window.NativeWindow.Close();
                    }
                }
            }

            Cleanup();

            Shutdown();
        }

        private void Shutdown()
        {
            RenderManager.Shutdown();
            AssetManager.Shutdown();

            Logger.Shutdown();
        }

        protected virtual void Initialize()
        {

        }

        protected virtual void Update(float deltaTime)
        {
        }

        protected virtual void OnGUI()
        {
        }

        protected virtual void Cleanup()
        {

        }

        protected void Exit()
        {
            _isRunning = false;
        }
    }
}
