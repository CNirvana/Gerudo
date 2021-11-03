using Veldrid;

namespace Gerudo
{
    public static class GlobalBuffers
    {
        public static PerFrameBuffer PerFrameBuffer { get; private set; }

        public static PerDrawBuffer PerDrawBuffer { get; private set; }

        public static BoneDataBuffer BoneDataBuffer { get; private set; }

        internal static void Initialize()
        {
            CreatePerFrameBuffer();
            CreatePerDrawBuffer();
            CreateBoneDataBuffer();
        }

        private static void CreatePerFrameBuffer()
        {
            var layout = GraphicsContext.Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("PerFrameBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));

            BufferDescription desc = new BufferDescription()
            {
                SizeInBytes = (16 + 16 + 4 + 4) * 4,
                Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
            };
            var buffer = GraphicsContext.Factory.CreateBuffer(desc);

            var set = GraphicsContext.Factory.CreateResourceSet(new ResourceSetDescription(
                layout,
                buffer
            ));

            PerFrameBuffer = new PerFrameBuffer(layout, buffer, set);
        }

        private static void CreatePerDrawBuffer()
        {
            var layout = GraphicsContext.Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("PerDrawBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            BufferDescription desc = new BufferDescription()
            {
                SizeInBytes = 16 * 4,
                Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
            };
            var buffer = GraphicsContext.Factory.CreateBuffer(desc);

            var set = GraphicsContext.Factory.CreateResourceSet(new ResourceSetDescription(
                layout,
                buffer
            ));

            PerDrawBuffer = new PerDrawBuffer(layout, buffer, set);
        }

        private static void CreateBoneDataBuffer()
        {
            var layout = GraphicsContext.Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("BonesBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            BufferDescription desc = new BufferDescription()
            {
                SizeInBytes = 64 * 64,
                Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
            };
            var buffer = GraphicsContext.Factory.CreateBuffer(desc);

            var set = GraphicsContext.Factory.CreateResourceSet(new ResourceSetDescription(
                layout,
                buffer
            ));

            BoneDataBuffer = new BoneDataBuffer(layout, buffer, set);
        }

        public static void Cleanup()
        {
            PerFrameBuffer.Dispose();
            PerDrawBuffer.Dispose();
            BoneDataBuffer.Dispose();
        }
    }
}