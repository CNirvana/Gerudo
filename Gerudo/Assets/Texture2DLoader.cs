using Veldrid;
using Veldrid.ImageSharp;

namespace Gerudo
{
    public class Texture2DLoader : IAssetLoader
    {
        public IAsset Load(string path)
        {
            var image = new ImageSharpTexture(path);

            Texture texture = image.CreateDeviceTexture(GraphicsContext.Device, GraphicsContext.Factory);
            TextureView view = GraphicsContext.Factory.CreateTextureView(texture);

            var sampler = GraphicsContext.Factory.CreateSampler(new SamplerDescription(
                SamplerAddressMode.Clamp, SamplerAddressMode.Clamp, SamplerAddressMode.Clamp,
                SamplerFilter.MinLinear_MagLinear_MipPoint, null,
                0, 0, uint.MaxValue, 0, SamplerBorderColor.OpaqueWhite));

            return new Texture2D(view, sampler);
        }
    }
}