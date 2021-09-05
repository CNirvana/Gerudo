using Veldrid;
using Veldrid.Sdl2;

namespace Gerudo
{
    public static class Input
    {
        public static Keyboard Keyboard { get; private set; } = new Keyboard();

        public static Mouse Mouse { get; private set; } = new Mouse();

        internal static void UpdateFrameInput(InputSnapshot snapshot, Sdl2Window window)
        {
            Keyboard.Update(snapshot);
            Mouse.Update(snapshot, window);
        }
    }
}