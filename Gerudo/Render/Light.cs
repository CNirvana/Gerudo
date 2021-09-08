using Veldrid;

namespace Gerudo
{
    public abstract class Light
    {
        public RgbaFloat LightColor{ get; set; }

        public float Intensity { get; set; }
    }
}