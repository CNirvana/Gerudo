using System;
using Veldrid;

namespace Gerudo
{
    public abstract class UniformBuffer<T> : IDisposable
        where T : struct
    {
        public ResourceLayout Layout { get; private set; }

        public DeviceBuffer DeviceBuffer { get; private set; }

        public ResourceSet ResourceSet { get; private set; }

        public UniformBuffer(ResourceLayout layout, DeviceBuffer deviceBuffer, ResourceSet resourceSet)
        {
            this.Layout = layout;
            this.DeviceBuffer = deviceBuffer;
            this.ResourceSet = resourceSet;
        }

        public virtual void Update(CommandList cl, in T data)
        {
            cl.UpdateBuffer(DeviceBuffer, 0, data);
        }

        public void Dispose()
        {
            Layout.Dispose();
            DeviceBuffer.Dispose();
            ResourceSet.Dispose();
        }
    }
}