using System;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Gerudo
{
    public class RenderSystem : ISubSystem
    {
        public GraphicsDevice Device { get; private set; }

        internal Shader[] DefaultShaders { get; private set; }

        private CommandList _commandList;

        private ResourceLayout _perFrameLayout;

        private DeviceBuffer _perFrameBuffer;

        private ResourceSet _perFrameSet;

        private ResourceLayout _perDrawLayout;

        private DeviceBuffer _perDrawBuffer;

        private ResourceSet _perDrawSet;

        internal RenderSystem(GraphicsDevice device)
        {
            this.Device = device;
        }

        public void Startup()
        {
            CreateDefaultPipeline();
        }

        public void Update()
        {
        }

        public void Shutdown()
        {
            foreach (var shader in DefaultShaders)
            {
                shader.Dispose();
            }
            _commandList.Dispose();
            _perDrawLayout.Dispose();
            _perDrawBuffer.Dispose();
            _perDrawSet.Dispose();
            _perFrameLayout.Dispose();
            _perFrameBuffer.Dispose();
            _perFrameSet.Dispose();
            Device.Dispose();
        }

        private void CreateDefaultPipeline()
        {
            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(Shaders.COLOR_VERT),
                "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(Shaders.COLOR_FRAG),
                "main");

            DefaultShaders = Device.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            BufferDescription perFrameBufferDescription = new BufferDescription()
            {
                SizeInBytes = 32 * 4,
                Usage = BufferUsage.UniformBuffer
            };
            _perFrameBuffer = Device.ResourceFactory.CreateBuffer(perFrameBufferDescription);

            _perFrameLayout = Device.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("PerFrameBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                )
            );

            _perFrameSet = Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                _perFrameLayout,
                _perFrameBuffer
            ));

            BufferDescription perDrawBufferDescription = new BufferDescription()
            {
                SizeInBytes = 16 * 4,
                Usage = BufferUsage.UniformBuffer
            };
            _perDrawBuffer = Device.ResourceFactory.CreateBuffer(perDrawBufferDescription);

            _perDrawLayout = Device.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("PerDrawBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                )
            );

            _perDrawSet = Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                _perDrawLayout,
                _perDrawBuffer
            ));

            _commandList = Device.ResourceFactory.CreateCommandList();
        }

        public void Render(Scene scene)
        {
             _commandList.Begin();

            CameraData cameraData = scene.Camera.GetCameraData();
            _commandList.UpdateBuffer(_perFrameBuffer, 0, cameraData);

            _commandList.SetFramebuffer(Device.SwapchainFramebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(1f, 0);

            foreach (var renderer in scene.Renderers)
            {
                GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
                pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
                pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                    depthTestEnabled: true,
                    depthWriteEnabled: true,
                    comparisonKind: ComparisonKind.LessEqual);
                pipelineDescription.RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false);
                pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
                pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
                pipelineDescription.ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] { Vertex.Layout },
                    shaders: DefaultShaders);
                pipelineDescription.ResourceLayouts = new[] { _perFrameLayout, _perDrawLayout, renderer.Material.Layout };
                pipelineDescription.Outputs = Device.SwapchainFramebuffer.OutputDescription;

                var pipeline = Device.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);

                _commandList.SetPipeline(pipeline);

                _commandList.UpdateBuffer(_perDrawBuffer, 0, renderer.Transform.GetLocalToWorldMatrix());

                _commandList.SetGraphicsResourceSet(0, _perFrameSet);
                _commandList.SetGraphicsResourceSet(1, _perDrawSet);
                _commandList.SetGraphicsResourceSet(2, renderer.Material.MainTextureSet);

                foreach (var mesh in renderer.Model.meshes)
                {
                    _commandList.SetVertexBuffer(0, mesh.VertexBuffer);
                    _commandList.SetIndexBuffer(mesh.IndexBuffer, IndexFormat.UInt16);

                    _commandList.DrawIndexed(
                        indexCount: (uint)mesh.Indices.Length,
                        instanceCount: 1,
                        indexStart: 0,
                        vertexOffset: 0,
                        instanceStart: 0);
                }

                pipeline.Dispose();
            }

            _commandList.End();
            Device.SubmitCommands(_commandList);

            Device.SwapBuffers();
        }
    }
}