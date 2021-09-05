using Veldrid;

namespace Gerudo
{
    public class Texture2D : IAsset
    {
        public uint Width => View.Target.Width;

        public uint Height => View.Target.Height;

        internal TextureView View { get; private set; }

        internal Sampler Sampler { get; private set; }

        internal Texture2D(TextureView view, Sampler sampler)
        {
            this.View = view;
            this.Sampler = sampler;
        }
    }
}