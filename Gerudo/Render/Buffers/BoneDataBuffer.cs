using Veldrid;

namespace Gerudo
{
    public unsafe struct BoneTransformationData
    {
        public fixed float data[16 * 64];
    }

    public class BoneDataBuffer : UniformBuffer<BoneTransformationData>
    {
        public BoneDataBuffer(ResourceLayout layout, DeviceBuffer deviceBuffer, ResourceSet resourceSet)
            : base(layout, deviceBuffer, resourceSet)
        {
        }
    }
}