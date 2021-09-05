using Veldrid;
using Veldrid.ImageSharp;

namespace Gerudo
{
    public class Texture2DLoader : IAssetLoader
    {
        public IAsset Load(string path)
        {
            var device = Engine.Instance.RenderSystem.Device;

            var image = new ImageSharpTexture(path);

            Texture texture = image.CreateDeviceTexture(device, device.ResourceFactory);
            TextureView view = device.ResourceFactory.CreateTextureView(texture);

            var sampler = device.ResourceFactory.CreateSampler(new SamplerDescription(
                SamplerAddressMode.Clamp, SamplerAddressMode.Clamp, SamplerAddressMode.Clamp,
                SamplerFilter.MinLinear_MagLinear_MipPoint, null,
                0, 0, uint.MaxValue, 0, SamplerBorderColor.OpaqueWhite));

            return new Texture2D(view, sampler);
        }
    }
}