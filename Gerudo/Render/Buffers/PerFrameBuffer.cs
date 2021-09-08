using System.Numerics;
using Veldrid;

namespace Gerudo
{
    public struct PerFrameData
    {
        public Matrix4x4 viewMatrix;

        public Matrix4x4 projectionMatrix;

        public RgbaFloat lightColor;

        public Vector4 lightDirection;
    }

    public class PerFrameBuffer : UniformBuffer<PerFrameData>
    {
        public PerFrameBuffer(ResourceLayout layout, DeviceBuffer deviceBuffer, ResourceSet resourceSet)
            : base(layout, deviceBuffer, resourceSet)
        {
        }
    }
}