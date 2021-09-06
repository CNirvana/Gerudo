using Veldrid;

namespace Gerudo
{
    public static class GlobalBuffers
    {
        public static PerFrameBuffer PerFrameBuffer { get; private set; }

        public static PerDrawBuffer PerDrawBuffer { get; private set; }

        internal static void Initialize(GraphicsDevice device)
        {
            CreatePerFrameBuffer(device, device.ResourceFactory);
            CreatePerDrawBuffer(device, device.ResourceFactory);
        }

        private static void CreatePerFrameBuffer(GraphicsDevice device, ResourceFactory factory)
        {
            var layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("PerFrameBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            BufferDescription desc = new BufferDescription()
            {
                SizeInBytes = 32 * 4,
                Usage = BufferUsage.UniformBuffer
            };
            var buffer = factory.CreateBuffer(desc);

            var set = factory.CreateResourceSet(new ResourceSetDescription(
                layout,
                buffer
            ));

            PerFrameBuffer = new PerFrameBuffer(layout, buffer, set);
        }

        private static void CreatePerDrawBuffer(GraphicsDevice device, ResourceFactory factory)
        {
            var layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("PerDrawBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            BufferDescription desc = new BufferDescription()
            {
                SizeInBytes = 16 * 4,
                Usage = BufferUsage.UniformBuffer
            };
            var buffer = factory.CreateBuffer(desc);

            var set = factory.CreateResourceSet(new ResourceSetDescription(
                layout,
                buffer
            ));

            PerDrawBuffer = new PerDrawBuffer(layout, buffer, set);
        }

        public static void Cleanup()
        {
            PerFrameBuffer.Dispose();
            PerDrawBuffer.Dispose();
        }
    }
}