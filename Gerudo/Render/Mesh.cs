using System;
using Veldrid;

namespace Gerudo
{
    public class Mesh : IDisposable
    {
        public Vertex[] Vertices { get; private set; }

        public ushort[] Indices { get; private set; }

        public DeviceBuffer VertexBuffer { get; private set; }

        public DeviceBuffer IndexBuffer { get; private set; }

        internal Mesh(Vertex[] vertices, ushort[] indices)
        {
            this.Vertices = vertices;
            this.Indices = indices;
        }

        public static Mesh Create(Vertex[] vertices, ushort[] indices)
        {
            var mesh = new Mesh(vertices, indices);
            mesh.UpdateBuffer(Engine.Instance.RenderSystem.Device);
            return mesh;
        }

        internal void UpdateBuffer(GraphicsDevice device)
        {
            BufferDescription vbDescription = new BufferDescription(
                (uint)Vertices.Length * Vertex.SizeInBytes,
                BufferUsage.VertexBuffer);
            VertexBuffer = device.ResourceFactory.CreateBuffer(vbDescription);
            device.UpdateBuffer(VertexBuffer, 0, Vertices);

            BufferDescription ibDescription = new BufferDescription(
                (uint)Indices.Length * sizeof(ushort),
                BufferUsage.IndexBuffer);
            IndexBuffer = device.ResourceFactory.CreateBuffer(ibDescription);
            device.UpdateBuffer(IndexBuffer, 0, Indices);
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}