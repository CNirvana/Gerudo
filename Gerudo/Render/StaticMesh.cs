using Veldrid;

namespace Gerudo
{
    public class StaticMesh : Mesh
    {
        public Vertex[] Vertices { get; private set; }

        public ushort[] Indices { get; private set; }

        public override uint IndexCount => (uint)Indices.Length;

        internal StaticMesh(Vertex[] vertices, ushort[] indices)
        {
            this.Vertices = vertices;
            this.Indices = indices;
        }

        public static StaticMesh Create(Vertex[] vertices, ushort[] indices)
        {
            var mesh = new StaticMesh(vertices, indices);
            mesh.UpdateBuffer(GraphicsContext.Device);
            return mesh;
        }

        internal override void UpdateBuffer(GraphicsDevice device)
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
    }
}