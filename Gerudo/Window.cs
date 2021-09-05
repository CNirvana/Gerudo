using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Gerudo
{
    public class Window
    {
        public Sdl2Window NativeWindow { get; private set; }

        public int WindowWidth { get => NativeWindow.Width; }

        public int WindowHeight { get => NativeWindow.Height; }

        public bool CursorVisible
        {
            get => NativeWindow.CursorVisible;
            set
            {
                NativeWindow.CursorVisible = value;
                Sdl2Native.SDL_SetRelativeMouseMode(!value);
            }
        }

        internal bool Exist { get => NativeWindow.Exists; }

        internal Window(WindowCreateInfo windowCI)
        {
            this.NativeWindow = VeldridStartup.CreateWindow(ref windowCI);
            this.NativeWindow.Resizable = false;
        }
    }
}