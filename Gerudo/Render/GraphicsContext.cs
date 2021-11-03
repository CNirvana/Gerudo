using Veldrid;

namespace Gerudo
{
    public static class GraphicsContext
    {
        public static GraphicsDevice Device { get; set; }

        public static ResourceFactory Factory => Device.ResourceFactory;

        public static void Dispose()
        {
            Device.Dispose();
        }
    }
}