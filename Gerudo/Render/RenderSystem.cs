using System;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Gerudo
{
    public class RenderSystem : ISubModule
    {
        public GraphicsDevice Device { get; private set; }

        private CommandList _commandList;

        internal RenderSystem(GraphicsDevice device)
        {
            this.Device = device;
        }

        public void Startup()
        {
            GlobalBuffers.Initialize(Device);

            _commandList = Device.ResourceFactory.CreateCommandList();
        }

        public void Update()
        {
        }

        public void Shutdown()
        {
            GlobalBuffers.Cleanup();

            _commandList.Dispose();
            Device.Dispose();
        }

        public void Render(Scene scene)
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

            _commandList.SetFramebuffer(Device.SwapchainFramebuffer);
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
                    _commandList.SetVertexBuffer(0, mesh.VertexBuffer);
                    _commandList.SetIndexBuffer(mesh.IndexBuffer, IndexFormat.UInt16);

                    _commandList.DrawIndexed(
                        indexCount: (uint)mesh.Indices.Length,
                        instanceCount: 1,
                        indexStart: 0,
                        vertexOffset: 0,
                        instanceStart: 0);
                }
            }

            _commandList.End();
            Device.SubmitCommands(_commandList);

            Device.SwapBuffers();
        }
    }
}