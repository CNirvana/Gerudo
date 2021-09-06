using System.Numerics;
using Veldrid;

namespace Gerudo
{
    public struct PerDrawData
    {
        public Matrix4x4 modelMatrix;
    }

    public class PerDrawBuffer : UniformBuffer<PerDrawData>
    {
        public PerDrawBuffer(ResourceLayout layout, DeviceBuffer deviceBuffer, ResourceSet resourceSet)
            : base(layout, deviceBuffer, resourceSet)
        {
        }
    }
}