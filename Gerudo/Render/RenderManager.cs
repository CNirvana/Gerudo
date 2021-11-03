using System.Numerics;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace Gerudo
{
    public class RenderManager
    {
        private CommandList _commandList;

        private ImGuiRenderer _igRenderer;

        // private Pipeline _buildClusterComputePipeline;

        // private Shader _buildClusterComputeShader;

        // private Pipeline _deferredPipeline;

        private Window _window;

        internal RenderManager()
        {
        }

        internal bool Startup(Window window)
        {
            _window = window;

            InitializeGraphicsContext();
            InitializeClusterComputePipeline();
            InitializeDeferredPipeline();

            var outputDesc = new OutputDescription(
                new OutputAttachmentDescription(PixelFormat.D24_UNorm_S8_UInt),
                new [] { new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm)});
            _igRenderer = new ImGuiRenderer(GraphicsContext.Device, outputDesc, 1280, 720, ColorSpaceHandling.Linear);

            GlobalBuffers.Initialize();

            _commandList = GraphicsContext.Factory.CreateCommandList();

            return true;
        }

        private void InitializeGraphicsContext()
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions
            {
                Debug = true,
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true,
                SwapchainDepthFormat = PixelFormat.D24_UNorm_S8_UInt,
                ResourceBindingModel = ResourceBindingModel.Improved
            };
            GraphicsContext.Device = VeldridStartup.CreateGraphicsDevice(_window.NativeWindow, options, GraphicsBackend.Vulkan);
        }

        private void InitializeClusterComputePipeline()
        {
            // _buildClusterComputeShader = GraphicsContext.Factory.CreateFromSpirv(
            //     new ShaderDescription(
            //         ShaderStages.Compute,
            //         new byte[1],
            //         "main"
            //     )
            // );

            // ComputePipelineDescription pipelineDesc = new ComputePipelineDescription(
            //     _buildClusterComputeShader,
            //     new[]{  },
            //     1, 1, 1
            // );
            // _buildClusterComputePipeline = GraphicsContext.Factory.CreateComputePipeline(ref pipelineDesc);
        }

        private void InitializeDeferredPipeline()
        {
            // GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            // pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            // pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
            //     depthTestEnabled: true,
            //     depthWriteEnabled: true,
            //     comparisonKind: ComparisonKind.LessEqual);
            // pipelineDescription.RasterizerState = new RasterizerStateDescription(
            //     cullMode: FaceCullMode.Back,
            //     fillMode: PolygonFillMode.Solid,
            //     frontFace: FrontFace.Clockwise,
            //     depthClipEnabled: true,
            //     scissorTestEnabled: false);
            // pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            // pipelineDescription.ShaderSet = new ShaderSetDescription(
            //     vertexLayouts: new VertexLayoutDescription[] { Vertex.Layout },
            //     shaders: shaders);
            // pipelineDescription.ResourceLayouts = new[] {
            //     GlobalBuffers.PerFrameBuffer.Layout,
            //     GlobalBuffers.PerDrawBuffer.Layout,
            //     layout };
            // pipelineDescription.Outputs = new OutputDescription(
            //     new OutputAttachmentDescription(PixelFormat.D24_UNorm_S8_UInt),
            //     new [] { new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm) }
            // );
        }

        internal void ProcessInput(float deltaTime, InputSnapshot snapshot)
        {
            _igRenderer.Update(deltaTime, snapshot);
        }

        internal void Shutdown()
        {
            GlobalBuffers.Cleanup();

            _commandList.Dispose();
            _igRenderer.Dispose();

            GraphicsContext.Dispose();
        }

        internal void Render(Scene scene)
        {
             _commandList.Begin();

            CameraData cameraData = scene.Camera.GetCameraData();
            var perFrameData = new PerFrameData
            {
                viewMatrix = cameraData.viewMatrix,
                projectionMatrix = cameraData.projectionMatrix,
                lightColor = scene.MainLight.LightColor,
                lightDirection = new Vector4(scene.MainLight.Direction, 0)
            };
            GlobalBuffers.PerFrameBuffer.Update(_commandList, perFrameData);

            _commandList.SetFramebuffer(GraphicsContext.Device.SwapchainFramebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(1f, 0);

            foreach (var renderer in scene.Renderers)
            {
                _commandList.SetPipeline(renderer.Material.Pipeline);

                var perDrawData = new PerDrawData
                {
                    modelMatrix = renderer.Transform.GetLocalToWorldMatrix()
                };
                GlobalBuffers.PerDrawBuffer.Update(_commandList, perDrawData);

                _commandList.SetGraphicsResourceSet(0, GlobalBuffers.PerFrameBuffer.ResourceSet);
                _commandList.SetGraphicsResourceSet(1, GlobalBuffers.PerDrawBuffer.ResourceSet);
                _commandList.SetGraphicsResourceSet(2, renderer.Material.ResourceSet);

                foreach (var mesh in renderer.Model.meshes)
                {
                    mesh.Draw(_commandList);
                }
            }

            _igRenderer.Render(GraphicsContext.Device, _commandList);

            _commandList.End();
            GraphicsContext.Device.SubmitCommands(_commandList);
            GraphicsContext.Device.SwapBuffers();
        }
    }
}