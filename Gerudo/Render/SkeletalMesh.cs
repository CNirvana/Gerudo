using Veldrid;

namespace Gerudo
{
    public class SkeletalMesh : Mesh
    {
        public SkeletalVertex[] Vertices { get; private set; }

        public ushort[] Indices { get; private set; }

        public override uint IndexCount => (uint)Indices.Length;

        public Skeleton Skeleton { get; private set; }

        public BoneAnimInfo boneTransformationData;

        internal SkeletalMesh(SkeletalVertex[] vertices, ushort[] indices, Skeleton skeleton)
        {
            this.Vertices = vertices;
            this.Indices = indices;
            this.Skeleton = skeleton;
        }

        public static SkeletalMesh Create(SkeletalVertex[] vertices, ushort[] indices, Skeleton skeleton)
        {
            var mesh = new SkeletalMesh(vertices, indices, skeleton);
            mesh.UpdateBuffer(GraphicsContext.Device);
            return mesh;
        }

        internal override void UpdateBuffer(GraphicsDevice device)
        {
            BufferDescription vbDescription = new BufferDescription(
                (uint)Vertices.Length * SkeletalVertex.SizeInBytes,
                BufferUsage.VertexBuffer);
            VertexBuffer = device.ResourceFactory.CreateBuffer(vbDescription);
            device.UpdateBuffer(VertexBuffer, 0, Vertices);

            BufferDescription ibDescription = new BufferDescription(
                (uint)Indices.Length * sizeof(ushort),
                BufferUsage.IndexBuffer);
            IndexBuffer = device.ResourceFactory.CreateBuffer(ibDescription);
            device.UpdateBuffer(IndexBuffer, 0, Indices);
        }

        internal override void Draw(CommandList commandList)
        {
            GlobalBuffers.BoneDataBuffer.Update(commandList, boneTransformationData.GetTransformationData());

            commandList.SetGraphicsResourceSet(3, GlobalBuffers.BoneDataBuffer.ResourceSet);

            base.Draw(commandList);
        }
    }
}