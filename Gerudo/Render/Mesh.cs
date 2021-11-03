using System;
using Veldrid;

namespace Gerudo
{
    public abstract class Mesh : IDisposable
    {
        public abstract uint IndexCount { get; }

        internal DeviceBuffer VertexBuffer { get; set; }

        internal DeviceBuffer IndexBuffer { get; set; }

        internal abstract void UpdateBuffer(GraphicsDevice device);

        internal Mesh() { }

        internal virtual void Draw(CommandList commandList)
        {
            commandList.SetVertexBuffer(0, VertexBuffer);
            commandList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt16);

            commandList.DrawIndexed(
                indexCount: IndexCount,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}