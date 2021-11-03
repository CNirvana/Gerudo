using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Gerudo
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Vertex
    {
        public Vector3 position;

        public Vector2 texcoord;

        public Vector3 normal;

        public RgbaFloat color;

        public static readonly uint SizeInBytes = 4 * 8 + (uint)RgbaFloat.SizeInBytes;

        public static readonly VertexLayoutDescription Layout = new VertexLayoutDescription(
            new VertexElementDescription("aPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("aTexcoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("aNormal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("aColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        );

        public Vertex(Vector3 position, Vector2 texcoord, Vector3 normal, RgbaFloat color)
        {
            this.position = position;
            this.texcoord = texcoord;
            this.normal = normal;
            this.color = color;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SkeletalVertex
    {
        public Vector3 position;

        public Vector2 texcoord;

        public Vector3 normal;

        public RgbaFloat color;

        public UInt4 boneIndices;

        public Vector4 boneWeights;

        public static readonly uint SizeInBytes = 4 * 16 + (uint)RgbaFloat.SizeInBytes;

        public static readonly VertexLayoutDescription Layout = new VertexLayoutDescription(
            new VertexElementDescription("aPosition", VertexElementSemantic.Position, VertexElementFormat.Float3),
            new VertexElementDescription("aTexcoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("aNormal", VertexElementSemantic.Normal, VertexElementFormat.Float3),
            new VertexElementDescription("aColor", VertexElementSemantic.Color, VertexElementFormat.Float4),
            new VertexElementDescription("aBoneIndices", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4),
            new VertexElementDescription("aBoneWeights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        );

        public SkeletalVertex(Vector3 position, Vector2 texcoord, Vector3 normal, RgbaFloat color, UInt4 boneIndices, Vector4 boneWeights)
        {
            this.position = position;
            this.texcoord = texcoord;
            this.normal = normal;
            this.color = color;
            this.boneIndices = boneIndices;
            this.boneWeights = boneWeights;
        }
    }
}